using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

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


        }
    }
}
