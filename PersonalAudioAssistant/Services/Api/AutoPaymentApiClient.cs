using Android.Text;
using PersonalAudioAssistant.Contracts.AutoPayment;

namespace PersonalAudioAssistant.Services.Api
{
    public class AutoPaymentApiClient : BaseApiClient
    {
        public AutoPaymentApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<AutoPaymentResponse> CreateAutoPaymentAsync(string userId)
        {
            var url = $"{BaseUrl}AutoPayment";

            var request = new
            {
                UserId = userId
            };

            var autoPayment = await PostAsync<object, AutoPaymentResponse>(url, request);

            return autoPayment;
        }

        public async Task<AutoPaymentResponse> UpdateAutoPaymentAsync(string userId, int minTokenThreshold, int chargeAmount, bool isAutoPayment)
        {
            var url = $"{BaseUrl}AutoPayment";

            var request = new
            {
                UserId = userId,
                MinTokenThreshold = minTokenThreshold,
                ChargeAmount = chargeAmount,
                IsAutoPayment = isAutoPayment
            };

            var autoPayment = await PostAsync<object, AutoPaymentResponse>(url, request);

            return autoPayment;
        }

        public async Task<AutoPaymentResponse> GetAutoPaymentsByUserIdAsync(string userId)
        {
            var url = $"{BaseUrl}AutoPayment/byuserid";

            var request = new
            {
                UserId = userId
            };

            var autoPayment = await PostAsync<object, AutoPaymentResponse>(url, request);

            return autoPayment;
        }
    }
}
