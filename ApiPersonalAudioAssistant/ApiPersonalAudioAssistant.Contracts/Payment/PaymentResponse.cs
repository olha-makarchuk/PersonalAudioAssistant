namespace ApiPersonalAudioAssistant.Contracts.Payment
{
    public class PaymentResponse
    {
        public required string? Id { get; set; }
        public required string PaymentGatewayToken { get; set; }
        public required string MaskedCardNumber { get; set; }
        public required string UserId { get; set; }
        public required string DataExpired { get; set; }
    }
}
