namespace ApiPersonalAudioAssistant.Contracts.MoneyUsersUsed
{
    public class MoneyUsersUsedResponse
    {
        public required string Id { get; set; }
        public required string SubUserId { get; set; }
        public required decimal AmountMoney { get; set; }
    }
}
