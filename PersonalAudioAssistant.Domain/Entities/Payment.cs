
using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class Payment: BaseEntity
    {
        public required string PaymentGatewayToken { get; set; }
        public required string MaskedCardNumber { get; set; }
        public required string UserId { get; set; }
    }
}
