using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly CosmosDbContext _context;
        public MessageRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync(Message message, CancellationToken cancellationToken)
        {
            await _context.Messages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteMessagesByConversationIdAsync(string IdConversation, CancellationToken cancellationToken)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == IdConversation)
                .ToListAsync(cancellationToken);

            _context.Messages.RemoveRange(messages);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Message> GetLastMessageByConversationIdAsync(string conversationId, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Where(m => m.UserRole == "user")
                .OrderBy(m => m.DateTimeCreated)
                .LastOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(string conversationId, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.DateTimeCreated)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Message>> GetMessagesByConversationIdPaginatorAsync(string conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(c => c.DateTimeCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
