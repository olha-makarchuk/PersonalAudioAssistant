using System;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace PersonalAudioAssistant.Application.Services.Api
{
    public class VoicesApi
    {
        private readonly string _apiKey = "sk_b76dddc4f56ebe8383b50692c9120c637a2243fed76b371f";

        public async Task<List<Voice>> GetVoicesAsync()
        {
            var options = new RestClientOptions("https://api.elevenlabs.io")
            {
                ThrowOnAnyError = true
            };

            var client = new RestClient(options);
            var request = new RestRequest("v2/voices?include_total_count=true", Method.Get);
            request.AddHeader("xi-api-key", _apiKey);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(response.Content);
                var filteredVoices = myDeserializedClass.voices
                    .Where(v => v.fine_tuning?.state?.eleven_flash_v2_5 == "fine_tuned")
                    .ToList();

                return filteredVoices;
            }

            throw new Exception($"API call failed with status {response.StatusCode}: {response.Content}");
        }
    }
}

namespace PersonalAudioAssistant.Application.Services.Api
{
    public class FineTuning
    {
        public State state { get; set; }
    }

    public class Labels
    {
        public string accent { get; set; }
        public string description { get; set; }
        public string age { get; set; }
        public string gender { get; set; }
        public string use_case { get; set; }
    }

    public class Root
    {
        public List<Voice> voices { get; set; }
        public bool has_more { get; set; }
        public int total_count { get; set; }
        public object next_page_token { get; set; }
    }

    public class State
    {
        public string eleven_multilingual_v2 { get; set; }
        public string eleven_turbo_v2_5 { get; set; }
        public string eleven_flash_v2_5 { get; set; }
        public string eleven_v2_flash { get; set; }
        public string eleven_v2_5_flash { get; set; }
        public string eleven_turbo_v2 { get; set; }
        public string eleven_flash_v2 { get; set; }
        public string eleven_multilingual_sts_v2 { get; set; }
    }

    public class Voice
    {
        public string voice_id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public FineTuning fine_tuning { get; set; }
        public Labels labels { get; set; }
        public string preview_url { get; set; }
        public List<string> high_quality_base_model_ids { get; set; }
        public bool is_owner { get; set; }
    }
}
