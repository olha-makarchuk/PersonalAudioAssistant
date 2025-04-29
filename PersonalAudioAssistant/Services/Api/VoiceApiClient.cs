using PersonalAudioAssistant.Contracts.Voice;

namespace PersonalAudioAssistant.Services.Api
{
    public class VoiceApiClient : BaseApiClient
    {
        public VoiceApiClient(HttpClient httpClient) : base(httpClient) { }

        
        public async Task<List<VoiceResponse>> GetVoicesAsync(string userId)
        {
            var url = $"{BaseUrl}Voice/allvoices";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }

        public async Task<VoiceResponse?> GetVoiceByIdAsync(string voiceId)
        {
            var url = $"{BaseUrl}Voice/byid";

            var request = new
            {
                VoiceId = voiceId
            };

            var voice = await PostAsync<object, VoiceResponse>(url, request);

            return voice;
        }

        public async Task DeleteVoiceAsync(string idElevenlabs, string id)
        {
            var url = $"{BaseUrl}Voice";

            var request = new
            {
                IdElevenlabs = idElevenlabs,
                Id = id
            };

            await DeleteAsync(url, request);
        }

        public async Task<VoiceResponse?> UpdateVoiceAsync(string voiceId, string userId)
        {
            var url = $"{BaseUrl}Voice";

            var request = new
            {
                VoiceId = voiceId,
                UserId = userId
            };

            return await PutAsync<object, VoiceResponse>(url, request);
        }

        public async Task<string> CreateVoiceAsync(string voiceId, string name, string userId)
        {
            var url = $"{BaseUrl}Voice/byid";

            var request = new
            {
                VoiceId = voiceId,
                Name = name,
                UserId = userId
            };

            var voice = await PostAsync<object, string>(url, request);

            return voice;
        }
    }
}
