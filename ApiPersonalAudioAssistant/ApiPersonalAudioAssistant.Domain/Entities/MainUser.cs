using ApiPersonalAudioAssistant.Domain.Common;

namespace ApiPersonalAudioAssistant.Domain.Entities
{
    public class MainUser: BaseEntity
    {
        public  string? Email { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
