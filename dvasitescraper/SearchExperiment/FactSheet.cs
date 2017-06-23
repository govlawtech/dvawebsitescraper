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

        [IsFilterable]
        public string FactsheetId { get; set; }

        [IsSearchable]
        public string Purpose { get; set; }

        public DateTimeOffset? LastModified { get; set; }

        [IsSearchable]
        public List<string> Questions { get; set; }

        public List<string> RelatedFactsheets { get; set; }
        
        [IsFilterable]
        [IsSearchable]
        public List<string> CuratedKeyWords { get; set; }

        [IsSearchable]
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
                FullText = fullText
            };
        }
    
        private static string StripEndingBoilerPlate(HtmlDocument htmlDocument)
        {
            throw new NotImplementedException();
        }

        private static string ExtractPurposeText(HtmlDocument htmlDocument)
        {
            var purposeParas = htmlDocument.DocumentNode.SelectNodes("//h2[text() = 'Purpose'] ").FirstOrDefault();
            if (purposeParas == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            var contentPara = purposeParas.NextSibling;
            while (contentPara.Name != "h2")
            {
                sb.Append(contentPara.InnerText);
                contentPara = contentPara.NextSibling;
            }

            return sb.ToString();
        }

        private static string ExtractPlainTextFromHtmlFullText(string html)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
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
