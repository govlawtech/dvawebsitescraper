using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvaSiteScraper
{
    class Program
    {
        static void Main(string[] args)
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();
            HtmlDocument factSheetByKeywordIndexPage = Scraper.FetchHtmlDocument(new Uri(Properties.Settings.Default.factListByKeyWordPageUrl));

            var keywords = factSheetByKeywordIndexPage.DocumentNode.SelectNodes("//*[@id='block-views-factsheets-by-keyword-block']/div/div/div/div/h3/a").Select(n => n.InnerText);
            var factSheetNodes = factSheetByKeywordIndexPage.DocumentNode.SelectNodes("//*[@id='block-views-factsheets-by-keyword-block']/div/div/div/div/ul/li/div/span/a");
            var factSheetDataFromFactSheetIndexPage = from fn in factSheetNodes
                                                      let divForGroupOfFactSheets = fn.Ancestors().First(a => a.GetAttributeValue("class", "") == "item-list")
                                                      let keyWordForGroup = divForGroupOfFactSheets.SelectSingleNode("h3/a").InnerText
                                                      select new FactsheetItemFromIndexPage(HtmlEntity.DeEntitize(fn.InnerText), keyWordForGroup, fn.GetAttributeValue("href", ""));

            // get each fact sheet with a collection of keywords for that fact sheet

            var factSheetItemWithKeyWords = from fi in factSheetDataFromFactSheetIndexPage.Distinct(new FactSheetEqualityComparer())
                                            let keyWords = factSheetDataFromFactSheetIndexPage.Where(i => i.Title == fi.Title).Select(i => i.DvaKeyWord)
                                            select new
                                            {
                                                FactSheetName = fi.Title,
                                                Link = fi.Link,
                                                KeyWords = keyWords
                                            };

            var scrapingTasks = from s in factSheetItemWithKeyWords
                                let uri = new Uri(new Uri(Properties.Settings.Default.baseUrl),s.Link)
                                select getFactSheetData(s.FactSheetName, s.KeyWords.ToList(), uri);

            var top10Tasks = scrapingTasks.Take(10);

            List<ScrapedFactSheetData> scrapedResults = new List<ScrapedFactSheetData>();
            try
            {
                var results = scrapingTasks.AsParallel()
                    .Select(r => r.Result)
                    .ToList();

                scrapedResults = results;

            }
            catch (AggregateException e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }

            var jsonNodes = scrapedResults.Select(n => n.toJson());

            JArray allResults = new JArray(jsonNodes);

            var outputPath = new FileInfo(Properties.Settings.Default.outputPath);
            File.WriteAllText(outputPath.FullName, allResults.ToString(),Encoding.UTF8);
            Console.WriteLine($"Output: {outputPath.FullName}\n" +
                $"Size {BytesToString(outputPath.Length)}\n" +
                $"Seconds to scrape: {sw.Elapsed.TotalSeconds}\n" +
                $"Number of fact sheets: {scrapedResults.Count}");
        }
               

        private static async Task<ScrapedFactSheetData> getFactSheetData(string factSheetName, List<String> keywords, Uri uri)
        {
                var htmlDoc = await Scraper.FetchHtmlDocAsync(uri);
                Console.WriteLine($"Retrived: {uri.ToString()}");
                var contentNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='content']/div/div[@class='region region-content']");
                var contentNodeHtml = contentNode.OuterHtml;
                return new ScrapedFactSheetData(factSheetName, keywords, contentNodeHtml,uri.ToString());
        }

        //http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }



        private struct FactsheetItemFromIndexPage
        {
            public string Title { get; private set; }
            public string DvaKeyWord { get; private set; }
            public string Link { get; set; }

            public FactsheetItemFromIndexPage(string title, string dvaKeyWord, string link)
            {
                Title = title;
                DvaKeyWord = dvaKeyWord;
                Link = link;
            }

        }

        private class FactSheetEqualityComparer : IEqualityComparer<FactsheetItemFromIndexPage>
        {
          

            public bool Equals(FactsheetItemFromIndexPage x, FactsheetItemFromIndexPage y)
            {
                return x.Title == y.Title;
            }

            public int GetHashCode(FactsheetItemFromIndexPage obj)
            {
                return obj.GetHashCode(); 
            }
        }
    }
}
