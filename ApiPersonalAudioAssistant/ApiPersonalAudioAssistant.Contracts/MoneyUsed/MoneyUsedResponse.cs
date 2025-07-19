namespace ApiPersonalAudioAssistant.Contracts.MoneyUsed
{
    public class MoneyUsedResponse 
    {
        public required string Id { get; set; }
        public required DateTime DateTimeUsed { get; set; }
        public required decimal AmountMoney { get; set; }
        public required string MainUserId { get; set; }
    }
}
