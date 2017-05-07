using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DvaSiteScraper
{
    class Scraper
    {
        public static HtmlDocument FetchHtmlDocument(Uri uri)
        {
            var req = CreateWebRequest(uri);
            Trace.WriteLine($"Sending request for: {uri.ToString()}...");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var resp = (HttpWebResponse)req.GetResponse();
                sw.Stop();
                System.Diagnostics.Trace.WriteLine($"Received response in {sw.ElapsedMilliseconds} ms.");
                var respStream = resp.GetResponseStream();
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.Load(respStream,Encoding.UTF8);
                return htmlDocument;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"Exception for id:  {uri.ToString()}: {e.Message}");
                return null;
            }
        }

        private static HttpWebRequest CreateWebRequest(Uri uri)
        {
            var req = HttpWebRequest.Create(uri);
            req.Timeout = 10000;
            return (HttpWebRequest)req;
        }
    }
}
