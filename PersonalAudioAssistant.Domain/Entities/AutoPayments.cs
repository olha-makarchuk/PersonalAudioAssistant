using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class AutoPayments: BaseEntity
    {
        public string? IsAutoPayment { get; set; }
        public int MinTokenThreshold { get; set; }
        public int ChargeAmount { get; set; }
        public string? PaymentId { get; set; }
    }
}
