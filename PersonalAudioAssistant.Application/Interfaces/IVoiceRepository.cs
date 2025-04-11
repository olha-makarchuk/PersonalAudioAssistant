using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IVoiceRepository
    {
        Task<Voice> GetVoiceByIdAsync(string id, CancellationToken cancellationToken);
        Task<List<Voice>> GetAllVoicesByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task DeleteVoiceByUserIdAsync(string id, CancellationToken cancellationToken);
        Task CreateVoice(Voice voice, CancellationToken cancellationToken);
    }
}
