using ApiPersonalAudioAssistant.Domain.Common;

namespace ApiPersonalAudioAssistant.Domain.Entities
{
    public class MoneyUsersUsed: BaseEntity
    {
        public required string SubUserId { get; set; }
        public required decimal AmountMoney { get; set; }
    }
}
