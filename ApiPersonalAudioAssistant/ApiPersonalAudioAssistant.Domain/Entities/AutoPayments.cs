using ApiPersonalAudioAssistant.Domain.Common;

namespace ApiPersonalAudioAssistant.Domain.Entities
{
    public class AutoPayments: BaseEntity
    {
        public required bool IsAutoPayment { get; set; }
        public required int MinTokenThreshold { get; set; }
        public required int ChargeAmount { get; set; }
        public required string UserId { get; set; }
    }
}
