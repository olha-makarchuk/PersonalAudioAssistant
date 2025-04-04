using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IAppSettingsRepository
    {
        Task<AppSettings> GetSettingsByUserIdAsync(string id, CancellationToken cancellationToken);
        Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken);
        Task AddSettingsAsync(AppSettings settings, CancellationToken cancellationToken);
    }
}
