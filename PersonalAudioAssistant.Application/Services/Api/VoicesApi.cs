using Newtonsoft.Json;
using RestSharp;

namespace PersonalAudioAssistant.Application.Services.Api
{
    public class VoicesApi
    {
        private readonly string _apiKey = "sk_b76dddc4f56ebe8383b50692c9120c637a2243fed76b371f";

        public async Task<List<Voice>> GetVoicesAsync()
        {
            var allFilteredVoices = new List<Voice>();
            string? nextPageToken = null;
            int totalCount = 0;

            var options = new RestClientOptions("https://api.elevenlabs.io")
            {
                ThrowOnAnyError = true
            };
            var client = new RestClient(options);

            do
            {
                var request = new RestRequest("v2/voices?include_total_count=true", Method.Get);
                request.AddHeader("xi-api-key", _apiKey);

                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    request.AddQueryParameter("page_token", nextPageToken);
                }

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var result = JsonConvert.DeserializeObject<Root>(response.Content);

                    if (totalCount == 0)
                    {
                        totalCount = result.total_count;
                    }

                    var filteredVoices = result.voices
                        .Where(v => v.fine_tuning?.state?.eleven_flash_v2_5 == "fine_tuned")
                        .ToList();

                    allFilteredVoices.AddRange(filteredVoices);

                    nextPageToken = result.next_page_token.ToString();
                }
                else
                {
                    throw new Exception($"API call failed with status {response.StatusCode}: {response.Content}");
                }

            } while (allFilteredVoices.Count < totalCount);

            return allFilteredVoices;
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
