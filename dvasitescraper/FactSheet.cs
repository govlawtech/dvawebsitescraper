using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvaSiteScraper
{
    struct ScrapedFactSheetData
    {
        public string Title { get; private set; }
        public string DvaKeyWord { get; private set; }
        public HtmlNode Content { get; private set; }
        
        public ScrapedFactSheetData(string title, string dvaKeyWord, HtmlNode content)
        {
            Title = title;
            DvaKeyWord = dvaKeyWord;
            Content = content;
        }

        public override string ToString()
        {
            return $"{Title}";
        }

    }
}
