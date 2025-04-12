using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class VoiceRepository : IVoiceRepository
    {
        private readonly CosmosDbContext _context;
        public VoiceRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task CreateVoice(Voice voice, CancellationToken cancellationToken)
        {
            await _context.Voices.AddAsync(voice, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteVoiceByUserIdAsync(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);
            var voice = await _context.Voices.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            _context.Remove(voice);
        }

        public async Task<List<Voice>> GetAllVoicesByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            var a = await _context.Voices
                .Where(x => x.UserId == userId || x.UserId == null)
                .ToListAsync(cancellationToken);
            return a;
        }

        public async Task<Voice> GetVoiceByIdAsync(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);

            return await _context.Voices
                .FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
        }
    }
}
