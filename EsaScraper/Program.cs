using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsaScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.Load(args[0], Encoding.UTF8);
            var links = htmlDocument.DocumentNode.SelectNodes("//span[@class='factsheetinfo']/a").Select(a => a.InnerText);
            Console.WriteLine(String.Join(Environment.NewLine,links));
                
        }
    }
}
