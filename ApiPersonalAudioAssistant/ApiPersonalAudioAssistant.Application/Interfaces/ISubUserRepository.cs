using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface ISubUserRepository
    {
        Task<SubUser> GetUserByIdAsync(string id, CancellationToken cancellationToken);
        Task<SubUser> GetUserByNameAsync(string name, CancellationToken cancellationToken);
        Task<SubUser> GetUserByStartPhraseAsync(string userId, string name, CancellationToken cancellationToken);
        Task<SubUser> CreateUser(SubUser user, CancellationToken cancellationToken);
        Task UpdateUser(SubUser user, CancellationToken cancellationToken);
        Task AddUser(SubUser user, CancellationToken cancellationToken);
        Task DeleteUser(string id, CancellationToken cancellationToken);
        Task<List<SubUser>> GetAllUsersByUserId(string userId, CancellationToken cancellationToken);
    }
}
