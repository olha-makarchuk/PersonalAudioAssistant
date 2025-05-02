using PersonalAudioAssistant.Contracts.MoneyUsed;
using PersonalAudioAssistant.Contracts.MoneyUsersUsed;

namespace PersonalAudioAssistant.Services.Api
{
    public class MoneyUsersUsedApiClient : BaseApiClient
    {
        public MoneyUsersUsedApiClient(HttpClient httpClient) : base(httpClient) { }

        public async Task CreateMoneyUsersUsedAsync(string subUserId, double amountMoney)
        {
            var url = $"{BaseUrl}MoneyUsersUsed/create";

            var request = new
            {
                SubUserId = subUserId,
                AmountMoney = amountMoney
            };

            await PostAsync<object, string>(url, request);
        }

        public async Task<List<MoneyUsersUsedResponse>> GetMoneyUsersUsedAsync(string mainUserId)
        {
            var url = $"{BaseUrl}MoneyUsersUsed/get";

            var request = new
            {
                MainUserId = mainUserId
            };

            var moneyUsedlist = await PostAsync<object, List<MoneyUsersUsedResponse>>(url, request);

            return moneyUsedlist;
        }
    }
}
