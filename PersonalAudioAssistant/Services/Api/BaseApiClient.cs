using System.Text.Json;

namespace PersonalAudioAssistant.Services.Api
{
    public abstract class BaseApiClient
    {
        protected readonly HttpClient _httpClient;
        public string BaseUrl = $"http://192.168.0.155:8080/api/v1/";
        
        protected BaseApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Remove("Api-Version");
                _httpClient.DefaultRequestHeaders.Add("Api-Version", "1");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                // логування або обробка помилки
                return default;
            }
        }

        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Remove("Api-Version");
                _httpClient.DefaultRequestHeaders.Add("Api-Version", "1");

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                // логування
                return default;
            }
        }

        protected async Task<TResponse?> DeleteAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Remove("Api-Version");
                _httpClient.DefaultRequestHeaders.Add("Api-Version", "1");

                var json = JsonSerializer.Serialize(data);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(url),
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                // логування
                return default;
            }
        }


        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Remove("Api-Version");
                _httpClient.DefaultRequestHeaders.Add("Api-Version", "1");

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                // логування
                return default;
            }
        }
    }
}
