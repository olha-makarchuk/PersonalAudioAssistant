namespace PersonalAudioAssistant.Contracts.Auth
{
    public class MainUserRegisterResponse
    {
        public required string? Id { get; set; }
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime AccessExpiresAt { get; set; }
        public DateTime RefreshExpiresAt { get; set; }
    }
}
