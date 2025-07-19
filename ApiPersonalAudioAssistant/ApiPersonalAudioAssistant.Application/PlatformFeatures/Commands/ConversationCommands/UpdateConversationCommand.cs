using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands
{
    public class UpdateConversationCommand: IRequest<Unit>
    {
        public string ConversationId { get; set; }
        public string Description { get; set; }
    }

    public class UpdateConversationCommandHandler : IRequestHandler<UpdateConversationCommand, Unit>
    {
        private readonly IConversationRepository _conversationRepository;

        public UpdateConversationCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<Unit> Handle(UpdateConversationCommand request, CancellationToken cancellationToken = default)
        {
            var conversation = await _conversationRepository.GetConversationByIdAsync(request.ConversationId, cancellationToken);

            conversation.Description = request.Description;
            await _conversationRepository.UpdateConversationAsync(conversation, cancellationToken);

            return Unit.Value;
        }
    }
}
