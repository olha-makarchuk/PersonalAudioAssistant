using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface IVoiceRepository
    {
        Task<Voice> GetVoiceByIdAsync(string id, CancellationToken cancellationToken);
        Task<List<Voice>> GetAllVoicesByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task DeleteVoiceByUserIdAsync(string id, CancellationToken cancellationToken);
        Task CreateVoice(Voice voice, CancellationToken cancellationToken);
        Task UpdateVoiceAsync(Voice voice, CancellationToken cancellationToken);
        Task DeleteVoiceById(string id, CancellationToken cancellationToken);
    }
}
