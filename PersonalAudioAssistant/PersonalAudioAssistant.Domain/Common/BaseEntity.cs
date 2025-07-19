namespace PersonalAudioAssistant.Domain.Common
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
