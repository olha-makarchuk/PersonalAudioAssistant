using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class PaymentHistory: BaseEntity
    {
        public required DateTime Date { get; set; }
        public required decimal Amount { get; set; }
        public required string MaskedCardNumber { get; set; }
        public required string UserId { get; set; }
    }
}
