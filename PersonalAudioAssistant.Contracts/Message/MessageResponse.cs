namespace PersonalAudioAssistant.Contracts.Message
{
    public class MessageResponse
    {
        public string MessageId { get; set; }
        public string ConversationId { get; set; }
        public string Text { get; set; }
        public string UserRole { get; set; }
        public string AudioPath { get; set; }
    }
}
