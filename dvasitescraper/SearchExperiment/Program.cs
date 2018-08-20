using System;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Spatial;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace SearchExperiment
{
    class Program
    {
        private static readonly string indexFieldName = "IndexName";
        

        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            string indexName = configuration[indexFieldName];
            string pageViewDataCsvPath = configuration["FactsheetWebAnalyticsPath"];

            var data = File.ReadAllLines(pageViewDataCsvPath)
                .ToList()
                .Select(l => l.Split(',').ToList())
                .Select(i => new { RelativeUrl = i.ElementAt(0), Uniques = Convert.ToDouble(i.ElementAt(1)) })
                .ToDictionary(i => i.RelativeUrl, i => i.Uniques);

            Func<string, double> getUniquesForRelativeUrl = relativeUrl =>
             {
                 if (data.ContainsKey(relativeUrl))
                     return data[relativeUrl];
                 else return 0;
             };
                

            SearchServiceClient serviceClient = CreateSearchServiceClient(configuration);
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }

            
            CreateIndex(serviceClient, indexName);
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            String jsonString = File.ReadAllText(configuration["FactSheetJsonPath"]);
            IEnumerable<FactSheet> factsheets = DeserializeFactsheets(jsonString,getUniquesForRelativeUrl);

            UploadDocuments(indexClient, factsheets);



        }

        private static IEnumerable<FactSheet> DeserializeFactsheets(string jsonString,Func<string,double> pageViewProvider)
        {
            JArray jarray = JArray.Parse(jsonString);
            IEnumerable<FactSheet> factsheetNodes = jarray.Select(n => FactSheet.fromJson(n.Value<JObject>()))
                .Select(fs => fs.WithUniquePageViews(pageViewProvider(new UriBuilder(fs.Url).Path)));
            
            
            var factsheetsWithMoreThanZeroUpv = factsheetNodes.Where(f => f.UniquePageViews > 0).ToList();

            return factsheetNodes;
        }

        private static SearchServiceClient CreateSearchServiceClient(IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            return serviceClient;
        }

        private static SearchIndexClient CreateSearchIndexClient(IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string queryApiKey = configuration["SearchServiceQueryApiKey"];

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, configuration[indexFieldName], new SearchCredentials(queryApiKey));
            return indexClient;
        }

        private static void CreateIndex(SearchServiceClient serviceClient, string indexName)
        {
            var scoringProfile = BuildScoringProfile();

            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<FactSheet>(),
                ScoringProfiles = new [] {scoringProfile},
                DefaultScoringProfile = scoringProfile.Name
            };

            serviceClient.Indexes.Create(definition);
        }

        private static void UploadDocuments(ISearchIndexClient indexClient, IEnumerable<FactSheet> factsheets)
        {
            var batch = IndexBatch.Upload(factsheets);
            try
            {
                Console.WriteLine("Startin to index batch...");
                indexClient.Documents.Index(batch);
                Console.WriteLine("Done.");
                Console.ReadKey();

            }
            catch (IndexBatchException e)
            {

               var failed = e.IndexingResults.Where(r => !r.Succeeded);
                Console.WriteLine("Failed: " + failed.Count());
                foreach (var f in failed)
                {
                    Console.WriteLine("Failed: " + f.Key + ": " + f.ErrorMessage);
                }

                Console.ReadKey();
            }
        }

       private static ScoringProfile BuildScoringProfile()
        {

            var sp = new ScoringProfile("dvachatbotprofile1",
                
                new TextWeights(new Dictionary<string, double>()
                {
                    {"purpose", 2D},
                    {"curatedKeyWords",3D},
                    {"factsheetId",5D}
                }),
                new List<ScoringFunction>()
                {
                    new MagnitudeScoringFunction()
                    {
                        FieldName = "uniquePageViews",
                        Interpolation = ScoringFunctionInterpolation.Linear,
                        Boost = 2,
                        Parameters = new MagnitudeScoringParameters() {BoostingRangeStart = 0, BoostingRangeEnd = 25000, ShouldBoostBeyondRangeByConstant = true}
                    }
                }

                );
            return sp;
        }

       

    }
}