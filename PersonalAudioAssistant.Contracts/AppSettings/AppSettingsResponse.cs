namespace PersonalAudioAssistant.Contracts.AppSettings
{
    public class AppSettingsResponse
    {
        public required string? Id { get; set; }
        public required string Theme { get; set; }
        public required int Balance { get; set; }
        public required string? UserId { get; set; }
    }
}
