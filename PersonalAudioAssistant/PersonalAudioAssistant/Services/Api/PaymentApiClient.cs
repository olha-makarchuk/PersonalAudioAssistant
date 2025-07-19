using PersonalAudioAssistant.Contracts.Payment;

namespace PersonalAudioAssistant.Services.Api
{

    public class PaymentApiClient : BaseApiClient
    {
        public PaymentApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<List<PaymentResponse>> CreatePaymentAsync(string userId)
        {
            var url = $"{BaseUrl}Payment";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<PaymentResponse>>(url, request);

            return voice;
        }

        public async Task<List<PaymentResponse>> UpdatePaymentAsync(string userId, string paymentGatewayToken, string maskedCardNumber, string dataExpiredCard)
        {
            var url = $"{BaseUrl}Payment";

            var request = new
            {
                UserId = userId,
                PaymentGatewayToken = paymentGatewayToken,
                MaskedCardNumber = maskedCardNumber,
                DataExpiredCard = dataExpiredCard
            };

            var voice = await PutAsync<object, List<PaymentResponse>>(url, request);

            return voice;
        }

        public async Task<PaymentResponse> GetPaymentByUserIdAsync(string userId)
        {
            var url = $"{BaseUrl}Payment/byid";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, PaymentResponse>(url, request);

            return voice;
        }
    }
}
