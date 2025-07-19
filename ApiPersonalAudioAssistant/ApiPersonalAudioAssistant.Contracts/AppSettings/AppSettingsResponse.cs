namespace ApiPersonalAudioAssistant.Contracts.AppSettings
{
    public class AppSettingsResponse
    {
        public required string? Id { get; set; }
        public required string Theme { get; set; }
        public required decimal Balance { get; set; }
        public required string? UserId { get; set; }
    }
}
