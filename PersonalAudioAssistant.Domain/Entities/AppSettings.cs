using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class AppSettings: BaseEntity
    {
        public required string Theme { get; set; }
        public required int Balance { get; set; }
        public required string? UserId { get; set; }
    }
}
