namespace PersonalAudioAssistant.Contracts.PaymentHistory
{
    public class PaymentHistoryResponse
    {
        public required string maskedCardNumber { get; set; }
        public required DateTime dataTimePayment { get; set; }
        public required string description { get; set; }
        public required decimal amount { get; set; }
    }
}
