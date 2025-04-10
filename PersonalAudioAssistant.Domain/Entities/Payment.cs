
using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class Payment: BaseEntity
    {
        public string? PaymentGatewayToken { get; set; }
        public string? MaskedCardNumber { get; set; }
        public string? UserId { get; set; }
    }
}
