using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public string? Description { get; set; }
        //public string? LastRequestId { get; set; }
        public string SubUserId { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}
