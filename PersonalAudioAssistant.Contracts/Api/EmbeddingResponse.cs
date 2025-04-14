using Newtonsoft.Json;

namespace PersonalAudioAssistant.Contracts.Api
{
    public class EmbeddingResponse
    {
        [JsonProperty("embedding")]
        public List<List<double>> Embedding { get; set; }
    }
}
