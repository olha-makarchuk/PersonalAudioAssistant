using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands
{
    public class DeleteConversationCommand : IRequest
    {
        public string IdConversation { get; set; }
    }

    public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand>
    {
        private readonly IConversationRepository _conversationRepository;

        public DeleteConversationCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken = default)
        {
            var conversation = await _conversationRepository.GetConversationByIdAsync(request.IdConversation, cancellationToken);
            if (conversation == null)
            {
                throw new Exception("Conversation with this Id not exists.");
            }

            await _conversationRepository.DeleteConversationAsync(conversation, cancellationToken);
        }
    }
}
