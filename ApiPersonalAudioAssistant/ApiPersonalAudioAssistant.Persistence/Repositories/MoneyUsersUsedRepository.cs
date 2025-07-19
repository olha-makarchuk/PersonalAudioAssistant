using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
{
    public class MoneyUsersUsedRepository : IMoneyUsersUsedRepository
    {
        private readonly CosmosDbContext _context;
        public MoneyUsersUsedRepository(CosmosDbContext context)
        {
            _context = context;
        }


        public async Task<List<MoneyUsersUsed>> GetMoneyUsersUsedByRangeSubUsersIdAsync(List<SubUser> subUsers, CancellationToken cancellationToken)
        {
            var subUserIds = subUsers.Select(s => s.Id.ToString()).ToList();

            return await _context.MoneyUsersUsed
                .Where(mu => subUserIds.Contains(mu.SubUserId))
                .ToListAsync(cancellationToken);
        }

        public async Task<MoneyUsersUsed> GetMoneyUsersUsedbySubUserIdAsync(string subUserId, CancellationToken cancellationToken)
        {
            return await _context.MoneyUsersUsed
                .FirstOrDefaultAsync(mu => mu.SubUserId == subUserId, cancellationToken);
        }

        public async Task AddMoneyUsersUsedAsync(MoneyUsersUsed moneyUsersUsed, CancellationToken cancellationToken)
        {
            await _context.MoneyUsersUsed.AddAsync(moneyUsersUsed, cancellationToken);
            _context.SaveChanges();
        }

        public async Task UpdateMoneyUsersUsedAsync(MoneyUsersUsed moneyUsersUsed, CancellationToken cancellationToken)
        {
            _context.MoneyUsersUsed.Update(moneyUsersUsed);
            _context.SaveChanges();
        }
    }
}
