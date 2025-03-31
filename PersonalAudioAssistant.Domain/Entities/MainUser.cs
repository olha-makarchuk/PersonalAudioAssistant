using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class MainUser: BaseEntity
    {
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
    }
}
