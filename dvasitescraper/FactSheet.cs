﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DvaSiteScraper
{
    struct ScrapedFactSheetData
    {
        public string Title { get; private set; }

        public string Content { get; private set; }

        public List<string> Keywords { get; private set; }
        
        public string Url { get; private set; }

        public ScrapedFactSheetData(string title, List<string> keywords, string html, string url)
        {
            Title = title;
            Content = html;
            Keywords = keywords;
            Url = url;
        }

        public JObject toJson()
        {
            return new JObject(
                new JProperty("title", Title),
                new JProperty("url", Url),
                new JProperty("keywordsFromIndexPage",
                    new JArray(Keywords)),
                new JProperty("factSheetHtml", Content));
        }

        public override string ToString()
        {
            return $"{Title}";
        }
        
    }
}
