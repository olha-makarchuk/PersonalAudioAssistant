using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text.Json;

namespace PersonalAudioAssistant.Application.Services
{
    public class ElevenlabsApi
    {
        private string _apiKey = "sk_b76dddc4f56ebe8383b50692c9120c637a2243fed76b371f";
        private string _baseUrl = "https://api.elevenlabs.io";
        private string _model = "eleven_flash_v2_5";

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


        public async Task<ElevenlabsApiResponse> CloneVoiceAsync(string voiceName, string filePath)
        {
            bool removeBackgroundNoise = true;
            var client = new RestClient(_baseUrl);
            var request = new RestRequest("/v1/voices/add", Method.Post);
            request.AddHeader("xi-api-key", _apiKey);

            request.AlwaysMultipartFormData = true;
            request.AddParameter("name", voiceName);
            request.AddParameter("remove_background_noise", removeBackgroundNoise.ToString().ToLower());

            request.AddFile("files", filePath, "audio/mpeg");

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Voice upload failed: {response.StatusCode} - {response.Content}");
            }

            using JsonDocument doc = JsonDocument.Parse(response.Content!);
            if (doc.RootElement.TryGetProperty("voice_id", out JsonElement idElement) &&
                idElement.ValueKind == JsonValueKind.String)
            {
                var responseAPI = new ElevenlabsApiResponse()
                {
                    VoiceId = idElement.GetString()!,
                    VoiceName = voiceName
                };

                return responseAPI;
            }

            throw new Exception("voice_id not found in the response.");
        }
    }

    public class ElevenlabsApiResponse
    {
        public string VoiceId { get; set; }
        public string VoiceName { get; set; }
    }
}