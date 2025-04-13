namespace PersonalAudioAssistant.Contracts.Auth
{
    public class TokenApiResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
