using ApiPersonalAudioAssistant.Domain.Common;

namespace ApiPersonalAudioAssistant.Domain.Entities
{
    public class PaymentHistory: BaseEntity
    {
        public required DateTime DateTimePayment { get; set; }
        public required decimal Amount { get; set; }
        public required string MaskedCardNumber { get; set; }
        public required string UserId { get; set; }
        public required string Description { get; set; }
    }
}
