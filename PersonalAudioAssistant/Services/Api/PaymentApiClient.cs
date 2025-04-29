using PersonalAudioAssistant.Contracts.Voice;

namespace PersonalAudioAssistant.Services.Api
{

    public class PaymentApiClient : BaseApiClient
    {
        public PaymentApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<List<VoiceResponse>> CreatePaymentAsync(string userId)
        {
            var url = $"{BaseUrl}Voice";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }

        public async Task<List<VoiceResponse>> UpdatePaymentAsync(string userId, string paymentGatewayToken, string maskedCardNumber, string dataExpiredCard)
        {
            var url = $"{BaseUrl}Voice";

            var request = new
            {
                UserId = userId,
                PaymentGatewayToken = paymentGatewayToken,
                MaskedCardNumber = maskedCardNumber,
                DataExpiredCard = dataExpiredCard
            };

            var voice = await PutAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }

        public async Task<List<VoiceResponse>> GetPaymentByUserIdAsync(string userId)
        {
            var url = $"{BaseUrl}Voice/byid";

            var request = new
            {
                UserId = userId
            };

            var voice = await PostAsync<object, List<VoiceResponse>>(url, request);

            return voice;
        }
    }
}
