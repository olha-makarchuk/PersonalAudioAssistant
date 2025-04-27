using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands
{
    public class UpdateConversationCommand: IRequest
    {
        public string ConversationId { get; set; }
        public string Description { get; set; }
    }

    public class UpdateConversationCommandHandler : IRequestHandler<UpdateConversationCommand>
    {
        private readonly IConversationRepository _conversationRepository;

        public UpdateConversationCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task Handle(UpdateConversationCommand request, CancellationToken cancellationToken = default)
        {
            var conversation = await _conversationRepository.GetConversationByIdAsync(request.ConversationId, cancellationToken);

            conversation.Description = request.Description;
            await _conversationRepository.UpdateConversationAsync(conversation, cancellationToken);
        }
    }
}
