using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface IConversationRepository
    {
        Task<Conversation> GetConversationByIdAsync(string Id, CancellationToken cancellationToken);
        Task<List<Conversation>> GetConversationsByUserIdPaginatorAsync(string subUserId, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<List<Conversation>> GetConversationsByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task UpdateConversationAsync(Conversation conversation, CancellationToken cancellationToken);
        Task AddConversationAsync(Conversation conversation, CancellationToken cancellationToken);
        Task DeleteConversationAsync(Conversation conversation, CancellationToken cancellationToken);
    }
}
