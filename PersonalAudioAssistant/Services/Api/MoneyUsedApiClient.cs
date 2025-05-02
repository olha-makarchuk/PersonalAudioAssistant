using PersonalAudioAssistant.Contracts.MoneyUsed;

namespace PersonalAudioAssistant.Services.Api
{
    public class MoneyUsedApiClient : BaseApiClient
    {
        public MoneyUsedApiClient(HttpClient httpClient) : base(httpClient) { }

        public async Task CreateMoneyUsedAsync(string mainUserId, string subUserId, decimal amountMoney)
        {
            var url = $"{BaseUrl}MoneyUsed/create";

            var request = new
            {
                MainUserId = mainUserId,
                SubUserId = subUserId,
                AmountMoney = amountMoney
            };

            await PostAsync<object, string>(url, request);
        }

        public async Task<List<MoneyUsedResponse>> GetMoneyUsedAsync(string mainUserId)
        {
            var url = $"{BaseUrl}MoneyUsed/get";

            var request = new
            {
                MainUserId = mainUserId
            };

            var moneyUsedlist = await PostAsync<object, List<MoneyUsedResponse>>(url, request);

            return moneyUsedlist;
        }
    }
}
