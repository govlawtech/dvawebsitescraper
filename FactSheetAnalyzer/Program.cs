﻿using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            var numSheets = o.Count();
            Console.WriteLine("Number of sheets: " + numSheets);

            Console.WriteLine("Number of key words: " + keywords.Count());
            var factSheetNumbersForEachKeyWord = from kw in keywords
                                                 let fsCount = o.Where(i => i["keywordsFromIndexPage"].Select(n => n.Value<String>()).Contains(kw)).Count()
                                                 select new
                                                 {
                                                     KeyWord = kw,
                                                     FsCount = fsCount
                                                 };
            var keywordCount = factSheetNumbersForEachKeyWord.Count();
            var topKeyWords = factSheetNumbersForEachKeyWord.OrderByDescending(i => i.FsCount);

            topKeyWords.ToList().ForEach(kw => Console.WriteLine($"{kw.KeyWord} & {kw.FsCount} \\\\"));

            File.WriteAllLines(Properties.Settings.Default.keywordsList, keywords, Encoding.UTF8);

            var wordCount = o.Select(fs => fs["factSheetHtml"].Value<String>()).Sum(words =>
            {
                var asHtml = HtmlNode.CreateNode(words);
                var innerText = asHtml.InnerText;
                return innerText.Split(' ', '\n', '\r').Where(i => !String.IsNullOrEmpty(i)).Count();
            });
                        
            Console.WriteLine("Word count: " + wordCount);

            var factSheetHtmls = o.Select(i => new
            {
                Title = i["title"].Value<string>(),
                BaldHtmlDoc =  createBaldFactSheetWebPage(i["factSheetHtml"].Value<string>())
            });

            Func<string,string> getFactsheetCode = (t =>
            {
                var code = t.Split(' ')[1];
                return code;
            });

            Directory.CreateDirectory(Properties.Settings.Default.baldFactSheetsOutputDicr);

            factSheetHtmls.ToList().ForEach(fs =>
            {
                var code = getFactsheetCode(fs.Title); 
                var outputPath = Path.Combine(Properties.Settings.Default.baldFactSheetsOutputDicr,$"{code}.html");
                fs.BaldHtmlDoc.Save(outputPath);
            });

        }
        

        private static HtmlDocument createBaldFactSheetWebPage(String factSheetDiv)
        {
            var withAbsoluteLinks = ReplaceRelativeLinksWithAbsolute(factSheetDiv);
            var factSheetDivWithMetadata = $"<!DOCTYPE html><html><head><meta charset=\"utf-8\"/></head>{withAbsoluteLinks}</html>";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(factSheetDivWithMetadata));
            HtmlDocument output = new HtmlDocument();
            output.Load(ms,Encoding.UTF8,false);
            StripEndingBoilerPlate(output); 
            return output;
        }

        private static string ReplaceRelativeLinksWithAbsolute(String factSheetDiv)
        {
            return Regex.Replace(factSheetDiv, @"<a href=""/", @"<a href=""https://www.dva.gov.au/");
        }

        private static void StripEndingBoilerPlate(HtmlDocument htmlDocument)
        {
            // drop all following siblings of <h2>More Information</h2>
            var moreInfoHeading = htmlDocument.DocumentNode.SelectNodes("//h2[text() = 'More Information']");
            if (moreInfoHeading != null)
            {
                HtmlNode current = moreInfoHeading.First().NextSibling;
                while (current != null)
                {
                    var toRemove = current;
                    current = current.NextSibling;
                    toRemove.Remove();
                }
                moreInfoHeading.First().Remove();
            }

            // drop rate this page thing
            var ratingsDiv = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'field-name-field-rate-this-page')]");
            if (ratingsDiv != null)
                ratingsDiv.First().Remove();
        }
    }
}
