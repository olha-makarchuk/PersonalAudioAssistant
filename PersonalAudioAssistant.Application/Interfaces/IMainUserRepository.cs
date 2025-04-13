using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IMainUserRepository
    {
        Task<MainUser> GetUserByIdAsync(string id, CancellationToken cancellationToken);
        Task CreateUser(MainUser user, CancellationToken cancellationToken);
        Task UpdateUser(MainUser user, CancellationToken cancellationToken);
        Task<List<MainUser>> GetAllUsers(CancellationToken cancellationToken);
        Task<MainUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    }
}
