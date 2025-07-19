using PersonalAudioAssistant.Contracts.PaymentHistory;

namespace PersonalAudioAssistant.Services.Api
{
    public class PaymentHistoryApiClient : BaseApiClient
    {
        public PaymentHistoryApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<List<PaymentHistoryResponse>> GetPaymentHistoryByUserIdAsync(string userId)
        {
            var url = $"{BaseUrl}PaymentHistory";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<PaymentHistoryResponse>>(url, request);

            return voice;
        }
    }
}
