namespace PersonalAudioAssistant.Contracts.Payment
{
    public class PaymentResponse
    {
        public required string? id { get; set; }
        public required string paymentGatewayToken { get; set; }
        public required string maskedCardNumber { get; set; }
        public required string userId { get; set; }
        public required string dataExpired { get; set; }
    }
}
