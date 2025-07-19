namespace ApiPersonalAudioAssistant.Contracts.AutoPayment
{
    public class AutoPaymentResponse
    {
        public required string? Id { get; set; }
        public required bool IsAutoPayment { get; set; }
        public required int MinTokenThreshold { get; set; }
        public required int ChargeAmount { get; set; }
        public required string UserId { get; set; }
    }
}
