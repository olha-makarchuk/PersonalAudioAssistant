using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    class MainUserRepository : IMainUserRepository
    {
        public Task<MainUser> CreateUser(MainUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUser(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MainUser>> GetAllUsers(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MainUser> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MainUser> GetUserByNameAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MainUser> UpdateUser(MainUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
