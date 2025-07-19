using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetMessagesByConversationIdAsync(string conversationId, CancellationToken cancellationToken); 
        Task<List<Message>> GetMessagesByConversationIdPaginatorAsync(string conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<Message> GetLastMessageByConversationIdAsync(string conversationId, CancellationToken cancellationToken);
        Task AddMessageAsync(Message message, CancellationToken cancellationToken);
        Task DeleteMessagesByConversationIdAsync(string IdConversation, CancellationToken cancellationToken);
    }
}
