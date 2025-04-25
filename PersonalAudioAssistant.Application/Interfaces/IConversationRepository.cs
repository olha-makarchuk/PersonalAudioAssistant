using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IConversationRepository
    {
        Task<Conversation> GetConversationByIdAsync(string Id, CancellationToken cancellationToken);
        Task<List<Conversation>> GetConversationsByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task UpdateConversationAsync(Conversation conversation, CancellationToken cancellationToken);
        Task AddConversationAsync(Conversation conversation, CancellationToken cancellationToken);
        Task DeleteConversationAsync(Conversation conversation, CancellationToken cancellationToken);
    }
}
