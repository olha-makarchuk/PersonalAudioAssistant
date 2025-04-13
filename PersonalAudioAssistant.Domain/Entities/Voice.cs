using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class Voice: BaseEntity
    {
        public required string VoiceId { get; set; }
        public required string Name { get; set; }
        public required string URL { get; set; }
        public required string Gender { get; set; }
        public required string Age { get; set; }
        public required string UseCase { get; set; } 
        public required string Description { get; set; }
        public required string UserId { get; set; }
    }
}
