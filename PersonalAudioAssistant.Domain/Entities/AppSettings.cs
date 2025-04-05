using PersonalAudioAssistant.Domain.Common;

namespace PersonalAudioAssistant.Domain.Entities
{
    public class AppSettings: BaseEntity
    {
        public string? Theme { get; set; }
        public string? Payment { get; set; }
        public int MinTokenThreshold { get; set; }
        public int ChargeAmount { get; set; }
        public int Balance { get; set; }
        public string? UserId { get; set; }
    }
}
