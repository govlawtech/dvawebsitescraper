using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QnALoader.QnaMaker
{
    /// <summary>
    /// Interacts with QnA Maker, from main ESA bot
    /// </summary>
    public class QnaMakerKb
    {
        private readonly Uri qnaBaseUri = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v2.0");

        private readonly string SubscriptionKey = ConfigurationManager.AppSettings["QnaSubscriptionKey"];

        // Sample HTTP Request:
        // POST /knowledgebases/create
        // Host: https://westus.api.cognitive.microsoft.com/qnamaker/v2.0
        // Ocp-Apim-Subscription-Key: {SubscriptionKey}
        // Content-Type: application/json
        // {"name" : "MRC01","urls": ["https://dvachatbotstorage.blob.core.windows.net/factsheets/MRC01.html"]}
        public string CreateKnowledgeBase(string name, string url)
        {
            var responseString = string.Empty;

            var uri = new UriBuilder($"{qnaBaseUri}/knowledgebases/create").Uri;

            var postBody = $"{{\"name\": \"{name}\",\"urls\": [\"{url}\"]}}";

            //Send the POST request
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Ocp-Apim-Subscription-Key", this.SubscriptionKey);

                try
                {
                    responseString = client.UploadString(uri, postBody);

                }
                catch (WebException we)
                {
                    throw;
                }
            }

            var result = GetKBIdFromResponse(responseString);

            return result.KBID;
        }

        // Convert Create service JSON response to object
        private QnaMakerCreateResult GetKBIdFromResponse(string responseString)
        {
            QnaMakerCreateResult response;
            try
            {
                response = JsonConvert.DeserializeObject<QnaMakerCreateResult>(responseString);
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }

            return response;
        }
    }
}