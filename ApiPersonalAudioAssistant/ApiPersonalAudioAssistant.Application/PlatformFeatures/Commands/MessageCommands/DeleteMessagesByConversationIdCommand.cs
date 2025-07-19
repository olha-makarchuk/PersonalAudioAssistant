using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MessageCommands
{
    public class DeleteMessagesByConversationIdCommand : IRequest<Unit>
    {
        public string IdConversation { get; set; }
    }

    public class DeleteMessagesByConversationIdCommandHandler : IRequestHandler<DeleteMessagesByConversationIdCommand, Unit>
    {
        private readonly IMessageRepository _messageRepository;

        public DeleteMessagesByConversationIdCommandHandler(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<Unit> Handle(DeleteMessagesByConversationIdCommand request, CancellationToken cancellationToken = default)
        {
            await _messageRepository.DeleteMessagesByConversationIdAsync(request.IdConversation, cancellationToken);

            return Unit.Value;
        }
    }
}
