namespace ApiPersonalAudioAssistant.Contracts.SubUser
{
    public class SubUserResponse
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public string? VoiceId { get; set; }
        public List<double>? UserVoice { get; set; }
        public string? UserId { get; set; }
        public byte[]? PasswordHash { get; set; }
        public required string PhotoPath { get; set; }
    }
}
