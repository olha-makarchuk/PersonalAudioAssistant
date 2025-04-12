using Newtonsoft.Json;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.Services
{
    public class ApiClientVoiceEmbedding : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClientVoiceEmbedding()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<double>> CreateVoiceEmbedding(Stream audioStream)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StreamContent(audioStream), "file", "recorded.wav");

                var apiUrl = "http://10.0.2.2:8000/embendding";
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Помилка при завантаженні: {response.StatusCode}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var embeddingResponse = JsonConvert.DeserializeObject<EmbeddingResponse>(jsonResponse);

                if (embeddingResponse?.Embedding != null && embeddingResponse.Embedding.Count > 0)
                {
                    return embeddingResponse.Embedding[0]; 
                }
                else
                {
                    Console.WriteLine("Помилка: Отримано порожній або неправильний embedding.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading voice: {ex.Message}");
                return null;
            }
        }



    }
    public class EmbeddingResponse
    {
        [JsonProperty("embedding")]
        public List<List<double>> Embedding { get; set; }
    }
}