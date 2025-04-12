using System.Net.Http.Headers;
using Newtonsoft.Json;
using PersonalAudioAssistant.Application.Interfaces;
using System.Collections.Generic; // Make sure this is included

namespace PersonalAudioAssistant.Application.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient()
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

                // Deserialize the JSON response into the EmbeddingResponse class
                var embeddingResponse = JsonConvert.DeserializeObject<EmbeddingResponse>(jsonResponse);

                // Check if the embedding was successfully deserialized and is not empty
                if (embeddingResponse?.Embedding != null && embeddingResponse.Embedding.Count > 0)
                {
                    // Flatten the list of lists into a single List<double>
                    return embeddingResponse.Embedding[0]; // Assuming the API always returns a single embedding vector
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
        // The "embedding" field in the JSON contains a list of lists of doubles
        [JsonProperty("embedding")]
        public List<List<double>> Embedding { get; set; }
    }
}