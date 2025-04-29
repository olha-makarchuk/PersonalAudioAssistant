using PersonalAudioAssistant.Contracts.Voice;

namespace PersonalAudioAssistant.Services.Api
{
    public class PaymentHistoryApiClient : BaseApiClient
    {
        public PaymentHistoryApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<List<VoiceResponse>> GetPaymentHistoryByUserIdAsync(string userId)
        {
            var url = $"{BaseUrl}Voice";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }
    }
}
