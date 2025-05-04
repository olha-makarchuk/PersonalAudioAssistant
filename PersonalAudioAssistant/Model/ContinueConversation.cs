using PersonalAudioAssistant.Contracts.SubUser;

namespace PersonalAudioAssistant.Model
{
    public class ContinueConversation
    {
        public string ConversationId { get; set; }
        public SubUserResponse SubUser { get; set; }
    }
}
