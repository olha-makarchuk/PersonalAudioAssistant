namespace ApiPersonalAudioAssistant.Contracts.PaymentHistory
{
    public class PaymentHistoryResponse
    {
        public required string MaskedCardNumber { get; set; }
        public required DateTime DataTimePayment { get; set; }
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
    }
}
