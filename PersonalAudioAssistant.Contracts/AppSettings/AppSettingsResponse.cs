namespace PersonalAudioAssistant.Contracts.AppSettings
{
    public class AppSettingsResponse
    {
        public required string? id { get; set; }
        public required string theme { get; set; }
        public required decimal balance { get; set; }
        public required string? userId { get; set; }
    }
}
