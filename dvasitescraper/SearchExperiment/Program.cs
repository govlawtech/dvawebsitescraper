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

            SearchServiceClient serviceClient = CreateSearchServiceClient(configuration);
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }

            
            CreateIndex(serviceClient, indexName);
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            String jsonString = File.ReadAllText(configuration["FactSheetJsonPath"]);
            IEnumerable<FactSheet> factsheets = DeserializeFactsheets(jsonString);

            UploadDocuments(indexClient, factsheets);



        }

        private static IEnumerable<FactSheet> DeserializeFactsheets(string jsonString)
        {
            JArray jarray = JArray.Parse(jsonString);
            IEnumerable<FactSheet> factsheetNodes = jarray.Select(n => FactSheet.fromJson(n.Value<JObject>()));
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
                indexClient.Documents.Index(batch);
            }
            catch (IndexBatchException e)
            {

               var failed = e.IndexingResults.Where(r => !r.Succeeded);
                Console.WriteLine("Failed: " + failed.Count());
                foreach (var f in failed)
                {
                    Console.WriteLine("Failed: " + f.Key + ": " + f.ErrorMessage);
                }
            }
        }

       private static ScoringProfile BuildScoringProfile()
        {

            var sp = new ScoringProfile("dvachatbotprofile1",
                new TextWeights(new Dictionary<string, double>()
                {
                    {"purpose", 3D},
                    {"curatedKeyWords",2D},
                    {"factsheetId",5D}

                }));
            return sp;
        }

    }
}