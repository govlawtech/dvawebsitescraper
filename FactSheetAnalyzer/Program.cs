using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactSheetAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            String json = File.ReadAllText(Properties.Settings.Default.inputJson,Encoding.UTF8);
            JArray o = JArray.Parse(json);
            var keywords = o.SelectMany(fs => fs["keywordsFromIndexPage"]).Select(i => i.Value<String>()).Distinct().OrderBy(i => i);
            Console.WriteLine("Number of key words: " + keywords.Count());
            var factSheetNumbersForEachKeyWord = from kw in keywords
                                                 let fsCount = o.Where(i => i["keywordsFromIndexPage"].Select(n => n.Value<String>()).Contains(kw)).Count()
                                                 select new
                                                 {
                                                     KeyWord = kw,
                                                     FsCount = fsCount
                                                 };
            var keywordCount = factSheetNumbersForEachKeyWord.Count();
            var topKeyWords = factSheetNumbersForEachKeyWord.OrderByDescending(i => i.FsCount).Take(20);

            topKeyWords.ToList().ForEach(kw => Console.WriteLine($"{kw.KeyWord} & {kw.FsCount} \\\\"));

            File.WriteAllLines(Properties.Settings.Default.keywordsList, keywords, Encoding.UTF8);

            var wordCount = o.Select(fs => fs["factSheetHtml"].Value<String>()).Sum(words =>
            {
                var asHtml = HtmlNode.CreateNode(words);
                var innerText = asHtml.InnerText;
                return innerText.Split(' ', '\n', '\r').Where(i => !String.IsNullOrEmpty(i)).Count();
            });

            Console.WriteLine("Word count: " + wordCount);
        }
    }
}
