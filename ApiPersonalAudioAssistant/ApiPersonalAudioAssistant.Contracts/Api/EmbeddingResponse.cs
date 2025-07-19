using Newtonsoft.Json;

namespace ApiPersonalAudioAssistant.Contracts.Api
{
    public class EmbeddingResponse
    {
        [JsonProperty("embedding")]
        public List<List<double>> Embedding { get; set; }
    }
}
