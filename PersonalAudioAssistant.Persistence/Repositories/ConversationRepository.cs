using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly CosmosDbContext _context;
        public ConversationRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task AddConversationAsync(Conversation conversation, CancellationToken cancellationToken)
        {
            await _context.Conversations.AddAsync(conversation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteConversationAsync(Conversation conversation, CancellationToken cancellationToken)
        {
            _context.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Conversation> GetConversationByIdAsync(string Id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(Id);
            var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            return conversation;
        }

        public async Task<List<Conversation>> GetConversationsByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            var conversation = await _context.Conversations
                .Where(conversation => conversation.SubUserId == userId)
                .OrderBy(m => m.DateTimeCreated)
                .ToListAsync(cancellationToken);

            return conversation;
        }

        public async Task<List<Conversation>> GetConversationsByUserIdPaginatorAsync(string subUserId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.Conversations
                .Where(c => c.SubUserId == subUserId)
                .OrderByDescending(c => c.DateTimeCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateConversationAsync(Conversation conversation, CancellationToken cancellationToken)
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
