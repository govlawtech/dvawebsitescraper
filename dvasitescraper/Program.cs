using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvaSiteScraper
{
    class Program
    {
        static void Main(string[] args)
        {

            HtmlDocument factSheetByKeywordIndexPage = Scraper.FetchHtmlDocument(new Uri(Properties.Settings.Default.factListByKeyWordPageUrl));

            var keywords = factSheetByKeywordIndexPage.DocumentNode.SelectNodes("//*[@id='block-views-factsheets-by-keyword-block']/div/div/div/div/h3/a").Select(n => n.InnerText);
            var factSheetNodes = factSheetByKeywordIndexPage.DocumentNode.SelectNodes("//*[@id='block-views-factsheets-by-keyword-block']/div/div/div/div/ul/li/div/span/a");
            var factSheetsGroupedByKeyword = from fn in factSheetNodes
                                             let divForGroupOfFactSheets = fn.Ancestors().First(a => a.GetAttributeValue("class", "") == "item-list")
                                             let keyWordForGroup = divForGroupOfFactSheets.SelectSingleNode("h3/a").InnerText
                                             select new
                                             {
                                                 KeyWord = keyWordForGroup,
                                                 FactSheetName = fn.InnerText,
                                                 FactSheetRelativeLink = fn.GetAttributeValue("href","")
                                             };
            
        }
    }
}
