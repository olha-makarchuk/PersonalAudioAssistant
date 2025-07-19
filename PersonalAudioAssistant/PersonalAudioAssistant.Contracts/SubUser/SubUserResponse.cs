namespace PersonalAudioAssistant.Contracts.SubUser
{
    public class SubUserResponse
    {
        public string? id { get; set; }
        public string? userName { get; set; }
        public string? startPhrase { get; set; }
        public string? endPhrase { get; set; }
        public string? endTime { get; set; }
        public string? voiceId { get; set; }
        public List<double>? userVoice { get; set; }
        public string? userId { get; set; }
        public byte[]? passwordHash { get; set; }
        public string photoPath { get; set; }
    }
}
