using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class Message: BaseEntity
    {
        public string ConversationId { get; set; }
        public string Text { get; }
        public string UserRole { get; set; }
    }
}
