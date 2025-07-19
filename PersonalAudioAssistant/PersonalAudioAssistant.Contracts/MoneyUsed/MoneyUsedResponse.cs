namespace PersonalAudioAssistant.Contracts.MoneyUsed
{
    public class MoneyUsedResponse
    {
        public string id { get; set; }
        public DateTime dateTimeUsed { get; set; }
        public decimal amountMoney { get; set; }
        public string mainUserId { get; set; }
    }
}
