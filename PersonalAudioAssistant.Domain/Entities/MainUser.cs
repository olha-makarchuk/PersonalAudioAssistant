using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class MainUser: BaseEntity
    {
        public required string? Email { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        public required string? RefreshToken { get; set; }
        public required DateTime RefreshTokenExpiryTime { get; set; }
    }
}
