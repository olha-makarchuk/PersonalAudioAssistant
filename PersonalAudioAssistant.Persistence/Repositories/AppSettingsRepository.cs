using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private readonly CosmosDbContext _context;
        public AppSettingsRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task AddSettingsAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            await _context.AppSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<AppSettings> GetSettingsByUserIdAsync(string Id, CancellationToken cancellationToken)
        {
            return await _context.AppSettings.FirstOrDefaultAsync(x => x.UserId == Id, cancellationToken);
        }

        public async Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            _context.AppSettings.Update(settings);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
