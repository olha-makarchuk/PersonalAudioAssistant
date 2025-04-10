using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class PaymentHistory: BaseEntity
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? MaskedCardNumber { get; set; }
        public string UserId { get; set; }
    }
}
