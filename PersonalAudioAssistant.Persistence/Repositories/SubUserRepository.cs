using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class SubUserRepository : ISubUserRepository
    {
        public Task<SubUser> CreateUser(SubUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUser(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<SubUser>> GetAllUsers(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<SubUser> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<SubUser> GetUserByNameAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<SubUser> UpdateUser(SubUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
