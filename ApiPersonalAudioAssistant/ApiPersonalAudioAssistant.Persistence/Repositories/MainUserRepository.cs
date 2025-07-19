using Microsoft.EntityFrameworkCore;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
{
    public class MainUserRepository : IMainUserRepository
    {
        private readonly CosmosDbContext _context;

        public MainUserRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task CreateUser(MainUser user, CancellationToken cancellationToken)
        {
            await _context.MainUsers.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<MainUser>> GetAllUsers(CancellationToken cancellationToken)
        {
            return await _context.MainUsers.ToListAsync(cancellationToken);
        }

        public async Task<MainUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _context.MainUsers.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
            return user!;
        }

        public async Task<MainUser> GetUserByIdAsync(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);

            var user = await _context.MainUsers.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            return user!;
        }

        public async Task UpdateUser(MainUser user, CancellationToken cancellationToken)
        {
            _context.MainUsers.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}