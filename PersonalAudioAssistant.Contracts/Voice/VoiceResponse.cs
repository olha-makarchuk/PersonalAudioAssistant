namespace PersonalAudioAssistant.Contracts.Voice
{
    public class VoiceResponse
    {
        public required string Id { get; set; }
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
