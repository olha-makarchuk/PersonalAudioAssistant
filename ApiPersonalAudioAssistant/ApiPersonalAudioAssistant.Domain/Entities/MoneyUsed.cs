using ApiPersonalAudioAssistant.Domain.Common;

namespace ApiPersonalAudioAssistant.Domain.Entities
{
    public class MoneyUsed: BaseEntity
    {
        public required DateTime DateTimeUsed { get; set; }
        public required decimal AmountMoney { get; set; }
        public required string MainUserId { get; set; }
    }
}
