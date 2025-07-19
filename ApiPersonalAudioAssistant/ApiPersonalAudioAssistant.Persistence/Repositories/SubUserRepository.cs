using ApiPersonalAudioAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
{
    public class SubUserRepository : ISubUserRepository
    {
        private readonly CosmosDbContext _context;
        public SubUserRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task<SubUser> CreateUser(SubUser user, CancellationToken cancellationToken)
        {
            await _context.SubUsers.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task DeleteUser(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);
            var user = await _context.SubUsers.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            _context.Remove(user!);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<SubUser>> GetAllUsersByUserId(string userId, CancellationToken cancellationToken)
        {
            var allUsers = await _context.SubUsers
                .Where(x => x.UserId == userId || x.UserId == null)
                .ToListAsync(cancellationToken);

            return allUsers;
        }

        public async Task<SubUser> GetUserByIdAsync(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);
            var user = await _context.SubUsers.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            return user!;
        }

        public async Task<SubUser> GetUserByNameAsync(string name, CancellationToken cancellationToken)
        {
            var user = await _context.SubUsers.FirstOrDefaultAsync(x => x.UserName == name, cancellationToken);
            return user!;
        }

        public async Task<SubUser> GetUserByStartPhraseAsync(string userId, string startPhrase, CancellationToken cancellationToken)
        {
            var user = await _context.SubUsers
                .FirstOrDefaultAsync(x => x.StartPhrase == startPhrase && x.UserId == userId, cancellationToken);
            return user!;
        }

        public async Task UpdateUser(SubUser user, CancellationToken cancellationToken)
        {
            _context.SubUsers.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddUser(SubUser user, CancellationToken cancellationToken)
        {
            await _context.SubUsers.AddAsync(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
