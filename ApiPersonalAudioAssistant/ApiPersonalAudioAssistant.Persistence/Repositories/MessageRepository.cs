using ApiPersonalAudioAssistant.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
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
            var a = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.UserRole == "user")
                .OrderByDescending(m => m.DateTimeCreated)
                .FirstOrDefaultAsync(cancellationToken);

            return a;
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(string conversationId, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.DateTimeCreated)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Message>> GetMessagesByConversationIdPaginatorAsync(
            string conversationId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            // 1) Сортуємо спочатку за DESC, щоб взяти останні pageSize повідомлень
            var page = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.DateTimeCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // 2) Повертаємо їх у хронологічному порядку (ASC) для коректного відображення в UI
            return page
                .OrderBy(m => m.DateTimeCreated)
                .ToList();
        }

    }
}
