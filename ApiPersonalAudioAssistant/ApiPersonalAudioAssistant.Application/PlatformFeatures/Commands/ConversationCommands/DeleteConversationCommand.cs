using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands
{
    public class DeleteConversationCommand : IRequest<Unit>
    {
        public string IdConversation { get; set; }
    }

    public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand, Unit>
    {
        private readonly IConversationRepository _conversationRepository;

        public DeleteConversationCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<Unit> Handle(DeleteConversationCommand request, CancellationToken cancellationToken = default)
        {
            var conversation = await _conversationRepository.GetConversationByIdAsync(request.IdConversation, cancellationToken);
            if (conversation == null)
            {
                throw new Exception("Conversation with this Id not exists.");
            }

            await _conversationRepository.DeleteConversationAsync(conversation, cancellationToken);

            return Unit.Value;
        }
    }
}
