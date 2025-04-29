
using PersonalAudioAssistant.Contracts.Voice;
using static Android.Content.Res.Resources;

namespace PersonalAudioAssistant.Services.Api
{
    public class AppSettingsApiClient : BaseApiClient
    {
        public AppSettingsApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<List<VoiceResponse>> CreateAppSettingsAsync(string userId)
        {
            var url = $"{BaseUrl}AppSettings";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }

        public async Task<List<VoiceResponse>> UpdateAppSettingsAsync(string userId, string balance, string theme)
        {
            var url = $"{BaseUrl}AppSettings";

            var request = new
            {
                UserId = userId,
                Balance = balance,
                Theme = theme
            };

            var voice = await PutAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }

        public async Task<List<VoiceResponse>> UpdateBalanceAsync(string userId, string balance, string theme)
        {
            var url = $"{BaseUrl}AppSettings/balance";

            var request = new
            {
                UserId = userId,
                Balance = balance,
                Theme = theme
            };

            var voice = await PostAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }

        public async Task<List<VoiceResponse>> GetSettingsByUserIdAsync(string userId)
        {
            var url = $"{BaseUrl}AppSettings/balance";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }
    }
}
