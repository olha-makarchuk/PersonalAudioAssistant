using ApiPersonalAudioAssistant.Domain.Common;

namespace ApiPersonalAudioAssistant.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public string? Description { get; set; }
        public string SubUserId { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}
