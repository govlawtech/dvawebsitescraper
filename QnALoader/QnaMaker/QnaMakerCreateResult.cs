
using Newtonsoft.Json;

namespace QnALoader.QnaMaker
{
    /// <summary>
    /// Class for QnA Maker Create response.
    /// 
    /// See https://westus.dev.cognitive.microsoft.com/docs/services/58994a073d9e04097c7ba6fe/operations/58994a073d9e041ad42d9baa
    /// </summary>
    class QnaMakerCreateResult
    {
        /// <summary>
        /// The created Knowledge Base ID
        /// </summary>
        [JsonProperty(PropertyName = "kbId")]
        public string KBID { get; set; }
    }
}
