using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IMainUserRepository
    {
        Task<MainUser> GetUserByIdAsync(int id, CancellationToken cancellationToken);
        Task<MainUser> GetUserByNameAsync(string name, CancellationToken cancellationToken);
        Task<MainUser> CreateUser(MainUser user, CancellationToken cancellationToken);
        Task<MainUser> UpdateUser(MainUser user, CancellationToken cancellationToken);
        Task DeleteUser(int id, CancellationToken cancellationToken);
        Task<List<MainUser>> GetAllUsers(CancellationToken cancellationToken);
    }
}
