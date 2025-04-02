using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Persistence.Repositories
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

        public async Task<MainUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _context.MainUsers.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        public async Task<MainUser?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.MainUsers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task UpdateUser(MainUser user, CancellationToken cancellationToken)
        {
            _context.MainUsers.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}