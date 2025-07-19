using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class Voice: BaseEntity
    {
        public required string VoiceId { get; set; }
        public required string Name { get; set; }
        public string URL { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string UseCase { get; set; } 
        public string Description { get; set; }
        public required string UserId { get; set; }
    }
}
