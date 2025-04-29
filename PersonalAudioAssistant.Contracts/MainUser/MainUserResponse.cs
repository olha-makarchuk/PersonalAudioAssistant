namespace PersonalAudioAssistant.Contracts.MainUser
{
    public class MainUserResponse
    {
        public required string? id { get; set; }
        public string? email { get; set; }
        public byte[]? passwordHash { get; set; }
        public byte[]? passwordSalt { get; set; }
        public string? refreshToken { get; set; }
        public DateTime refreshTokenExpiryTime { get; set; }
    }
}
