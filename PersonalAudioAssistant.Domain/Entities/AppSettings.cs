using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class AppSettings: BaseEntity
    {
        public required string theme { get; set; }
        public required decimal balance { get; set; }
        public required string? userId { get; set; }
    }
}
