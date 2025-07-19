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
            var voice = await _context.Voices.FirstOrDefaultAsync(x => x.UserId == id, cancellationToken);

            _context.Remove(voice!);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteVoiceById(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);
            var voice = await _context.Voices.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);

            _context.Remove(voice!);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Voice>> GetAllVoicesByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            var allVoices = await _context.Voices
                .Where(x => x.UserId == userId || x.UserId == null)
                .ToListAsync(cancellationToken);

            return allVoices;
        }

        public async Task<Voice> GetVoiceByIdAsync(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);
            var voice = await _context.Voices.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            return voice!;
        }

        public Task UpdateVoiceAsync(Voice voice, CancellationToken cancellationToken)
        {
            _context.Voices.Update(voice);
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
