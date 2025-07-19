using PersonalAudioAssistant.Contracts.AppSettings;

namespace PersonalAudioAssistant.Services.Api
{
    public class AppSettingsApiClient : BaseApiClient
    {
        public AppSettingsApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<List<AppSettingsResponse>> CreateAppSettingsAsync(string userId)
        {
            var url = $"{BaseUrl}AppSettings";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<AppSettingsResponse>>(url, request);

            return voice;
        }

        public async Task<List<AppSettingsResponse>> UpdateAppSettingsAsync(string userId, string balance, string theme)
        {
            var url = $"{BaseUrl}AppSettings";

            var request = new
            {
                UserId = userId,
                Balance = balance,
                Theme = theme
            };

            var voice = await PutAsync<object, List<AppSettingsResponse>>(url, request);

            return voice;
        }

        public async Task<List<AppSettingsResponse>> UpdateBalanceAsync(string userId, decimal rechargeAmountInput, string maskedCardNumber, string descriptionPayment)
        {
            var url = $"{BaseUrl}AppSettings/balance";

            var request = new
            {
                UserId = userId,
                RechargeAmountInput = rechargeAmountInput,
                MaskedCardNumber = maskedCardNumber,
                DescriptionPayment = descriptionPayment
            };

            var voice = await PostAsync<object, List<AppSettingsResponse>>(url, request);

            return voice;
        }

        public async Task<AppSettingsResponse> GetSettingsByUserIdAsync(string userId)
        {
            var url = $"{BaseUrl}AppSettings/getsettings";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, AppSettingsResponse>(url, request);

            return voice;
        }
    }
}
