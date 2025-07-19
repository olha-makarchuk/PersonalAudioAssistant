using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace ApiPersonalAudioAssistant.Application.Services
{
    public class ApiClientTokens
    {
        private readonly HttpClient _httpClient;
        private string _serverUrl;

        public ApiClientTokens(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _serverUrl = configuration["Server:BaseUrl"]!;
        }

        public async Task<int?> GetTokenCountAsync(string text)
        {
            try
            {
                var apiUrl = $"{_serverUrl}/tokenizer";
                var content = new StringContent(JsonConvert.SerializeObject(new { text }), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Помилка при отриманні кількості токенів: {response.StatusCode}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenData = JsonConvert.DeserializeObject<TokenCountResponse>(jsonResponse);

                return tokenData?.TokenCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при зверненні до API: {ex.Message}");
                return null;
            }
        }

        public class TokenCountResponse
        {
            [JsonProperty("token_count")]
            public int TokenCount { get; set; }
        }
    }
}
