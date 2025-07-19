using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.Api;

namespace ApiPersonalAudioAssistant.Application.Services
{
    public class ApiClientVoiceEmbedding : IApiClient
    {
        private readonly HttpClient _httpClient;
        private string _serverUrl;

        public ApiClientVoiceEmbedding(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _serverUrl = configuration["Server:BaseUrl"]!;
        }

        public async Task<List<double>> CreateVoiceEmbedding(Stream audioStream)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StreamContent(audioStream), "file", "recorded.wav");

                var apiUrl = $"{_serverUrl}/embendding";
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
}