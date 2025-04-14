using Microsoft.Extensions.Configuration;
using RestSharp;

namespace PersonalAudioAssistant.Application.Services
{
    public class TextToSpeechApi
    {
        private string _apiKey = "sk_b76dddc4f56ebe8383b50692c9120c637a2243fed76b371f";
        private string _baseUrl = "https://api.elevenlabs.io";
        private string _model = "eleven_flash_v2_5";

        /*
        public TextToSpeechApi(IConfiguration configuration)
        {
            _apiKey = configuration["ElevenLabsApi:ApiKey"]!;
            _baseUrl = configuration["ElevenLabsApi:BaseUrl"]!;
            _model = configuration["ElevenLabsApi:Model"]!;
        }*/

        public async Task<byte[]> ConvertTextToSpeechAsync(string voiceId, string text)
        {
            var client = new RestClient(_baseUrl);
            var request = new RestRequest($"/v1/text-to-speech/{voiceId}", Method.Post);
            request.AddHeader("xi-api-key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                text = text,
                model_id = _model
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful && response.RawBytes == null)
            {
                throw new Exception($"Error: {response.StatusCode} - {response.Content}");
            }

            return response.RawBytes!;
        }
    }
}
