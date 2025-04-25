using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.MessageCommands
{
    public class DeleteMessagesByConversationIdCommand : IRequest
    {
        public string IdConversation { get; set; }
    }

    public class DeleteMessagesByConversationIdCommandHandler : IRequestHandler<DeleteMessagesByConversationIdCommand>
    {
        private readonly IMessageRepository _messageRepository;

        public DeleteMessagesByConversationIdCommandHandler(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task Handle(DeleteMessagesByConversationIdCommand request, CancellationToken cancellationToken = default)
        {
            await _messageRepository.DeleteMessagesByConversationIdAsync(request.IdConversation, cancellationToken);
        }
    }
}
