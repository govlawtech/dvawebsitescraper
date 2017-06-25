using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Azure.Search.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;
using HtmlAgilityPack;
using System.Linq;

namespace SearchExperiment
{
    [SerializePropertyNamesAsCamelCase]
    public class FactSheet
    {
        
        [Key]
        public string Key { get; set; }

        [IsSearchable]
        [IsFilterable]
        public string FactsheetId { get; set; }

        [IsSearchable]
        [IsFilterable]
        [Analyzer(AnalyzerName.AsString.EnMicrosoft)]
        public string Purpose { get; set; }

        public DateTimeOffset? LastModified { get; set; }

        public List<string> Questions { get; set; }

        public List<string> RelatedFactsheets { get; set; }
        
        [IsFilterable]
        [IsSearchable]
        public List<string> CuratedKeyWords { get; set; }

        [IsSearchable]
        [IsFilterable]
        [Analyzer(AnalyzerName.AsString.EnMicrosoft)]
        public string FullText { get; set; }

        public static FactSheet fromJson(JObject factsheetNode)
        {
            var title = factsheetNode["title"].Value<String>();
            JArray keywordsArray = factsheetNode["keywordsFromIndexPage"].Value<JArray>();
            List<string> keywords = keywordsArray.ToObject<List<string>>();
            string fulltexthtml = factsheetNode["factSheetHtml"].ToObject<string>();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(fulltexthtml);

            string fullText = ExtractPlainTextFromHtmlFullText(fulltexthtml);
            string purpose = ExtractPurposeText(htmlDocument);           
            return new FactSheet()
            {
                Key = title.GetHashCode().ToString(),
                FactsheetId = title,
                CuratedKeyWords = keywords,
                FullText = fullText,
                Purpose = purpose
            };
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

        private static string ExtractPurposeText(HtmlDocument htmlDocument)
        {
            var purposeParasCollection = htmlDocument.DocumentNode.SelectNodes("//h2[text() = 'Purpose']");
            if (purposeParasCollection == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            var contentPara = purposeParasCollection.First().NextSibling;
            while (contentPara.Name != "h2")
            {
                sb.Append(contentPara.InnerText);
                contentPara = contentPara.NextSibling;
            }

            return sb.ToString().Trim();
        }

        private static string ExtractPlainTextFromHtmlFullText(string html)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            StripEndingBoilerPlate(htmlDocument);

            StringBuilder sb = new StringBuilder();
            foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//text()"))
            {
                string innerText = node.InnerText;
                if (!String.IsNullOrWhiteSpace(innerText))
                    sb.AppendLine(node.InnerText);
            }
            return sb.ToString();
        }
    }
}
