namespace ApiPersonalAudioAssistant.Contracts.Message
{
    public class MessageResponse
    {
        public string MessageId { get; set; }
        public string SubUserId { get; set; }
        public string ConversationId { get; set; }
        public string Text { get; set; }
        public string UserRole { get; set; }
        public string AudioPath { get; set; }
        public string LastRequestId { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}
