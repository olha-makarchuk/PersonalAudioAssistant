using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class Message: BaseEntity
    {
        public string ConversationId { get; set; }
        public string Text { get; set; }
        public string UserRole { get; set; }
        public string AudioPath { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}
