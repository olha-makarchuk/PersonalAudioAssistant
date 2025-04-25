using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetMessagesByConversationIdAsync(string conversationId, CancellationToken cancellationToken);
        Task AddMessageAsync(Message message, CancellationToken cancellationToken);
        Task DeleteMessagesByConversationIdAsync(string IdConversation, CancellationToken cancellationToken);
    }
}
