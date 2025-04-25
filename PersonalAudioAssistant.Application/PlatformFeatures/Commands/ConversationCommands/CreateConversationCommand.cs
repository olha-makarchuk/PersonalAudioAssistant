using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands
{
    public class CreateConversationCommand: IRequest<string>
    {
        public string Description { get; set; }
        public string SubUserId { get; set; }
    }
    public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, string>
    {
        private readonly IConversationRepository _conversationRepository;

        public CreateConversationCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<string> Handle(CreateConversationCommand request, CancellationToken cancellationToken = default)
        {
            var conversation = new Conversation
            {
                Description = request.Description,
                SubUserId = request.SubUserId,
                DateTimeCreated = DateTime.UtcNow
            };

            await _conversationRepository.AddConversationAsync(conversation, cancellationToken);

            return conversation.Id.ToString();
        }
    }
}
