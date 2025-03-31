using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface ISubUserRepository
    {
        Task<SubUser> GetUserByIdAsync(int id, CancellationToken cancellationToken);
        Task<SubUser> GetUserByNameAsync(string name, CancellationToken cancellationToken);
        Task<SubUser> CreateUser(SubUser user, CancellationToken cancellationToken);
        Task<SubUser> UpdateUser(SubUser user, CancellationToken cancellationToken);
        Task DeleteUser(int id, CancellationToken cancellationToken);
        Task<List<SubUser>> GetAllUsers(CancellationToken cancellationToken);
    }
}
