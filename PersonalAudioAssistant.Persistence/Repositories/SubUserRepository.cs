using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;

namespace PersonalAudioAssistant.Persistence.Repositories
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
            var user = await _context.SubUsers.FindAsync(new object[] { id }, cancellationToken);
            _context.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<SubUser>> GetAllUsers(CancellationToken cancellationToken)
        {
            return await _context.SubUsers.ToListAsync(cancellationToken);
        }

        public async Task<SubUser> GetUserByIdAsync(string id, CancellationToken cancellationToken)
        {
            return await _context.SubUsers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<SubUser> GetUserByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.SubUsers.FirstOrDefaultAsync(x => x.UserName == name, cancellationToken);
        }

        public async Task<SubUser> GetUserByStartPhraseAsync(string startPhrase, CancellationToken cancellationToken)
        {
            return await _context.SubUsers.FirstOrDefaultAsync(x => x.StartPhrase == startPhrase, cancellationToken);
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
