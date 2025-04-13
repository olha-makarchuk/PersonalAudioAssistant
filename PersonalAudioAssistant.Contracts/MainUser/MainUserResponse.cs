namespace PersonalAudioAssistant.Contracts.MainUser
{
    public class MainUserResponse
    {
        public required string? Id { get; set; }
        public string? Email { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
