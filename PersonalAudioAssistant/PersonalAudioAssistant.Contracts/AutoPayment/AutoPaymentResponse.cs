namespace PersonalAudioAssistant.Contracts.AutoPayment
{
    public class AutoPaymentResponse
    {
        public required string? id { get; set; }
        public required bool isAutoPayment { get; set; }
        public required int minTokenThreshold { get; set; }
        public required int chargeAmount { get; set; }
        public required string userId { get; set; }
    }
}
