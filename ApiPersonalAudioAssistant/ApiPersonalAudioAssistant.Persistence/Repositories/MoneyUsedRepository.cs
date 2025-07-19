using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
{
    public class MoneyUsedRepository: IMoneyUsedRepository
    {
        private readonly CosmosDbContext _context;
        public MoneyUsedRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task AddMoneyUsedAsync(MoneyUsed moneyUsed, CancellationToken cancellationToken)
        {
            await _context.MoneyUsed.AddAsync(moneyUsed, cancellationToken);
            _context.SaveChanges();
        }

        public async Task<List<MoneyUsed>> GetMoneyUsedByMainUserIdAsync(string mainUserId, CancellationToken cancellationToken)
        {
            var twelveMonthsAgo = DateTime.UtcNow.Date.AddMonths(-12);

            var all = await _context.MoneyUsed
                .Where(mu => mu.MainUserId == mainUserId)
                .ToListAsync(cancellationToken);

            return all
                .Where(mu => mu.DateTimeUsed.Date >= twelveMonthsAgo)
                .ToList();
        }


        public async Task UpdateMoneyUsedAsync(MoneyUsed moneyUsed, CancellationToken cancellationToken)
        {
            _context.MoneyUsed.Update(moneyUsed);
            _context.SaveChanges();
        }

        public async Task<MoneyUsed> GetMoneyUsedbyMainIdAndDateAsync(string moneyUsedId, DateTime dateTime, CancellationToken cancellationToken)
        {
            var dateOnly = dateTime.Date;
            var nextDay = dateOnly.AddDays(1);

            return await _context.MoneyUsed
                .Where(mu => mu.MainUserId == moneyUsedId && mu.DateTimeUsed >= dateOnly && mu.DateTimeUsed < nextDay)
                .FirstOrDefaultAsync(cancellationToken);
        }

    }
}
