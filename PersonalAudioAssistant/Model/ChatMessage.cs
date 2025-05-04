namespace PersonalAudioAssistant.Model
{
    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime DateTimeCreated { get; set; }
        public string? URL { get; set; }
        public string? SubUserPhoto { get; set; }
        public string? LastRequestId { get; set; }
        public bool ShowDate { get; set; }
    }
}
