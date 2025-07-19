using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands
{
    public class CreateConversationCommand: IRequest<ConversationIdResponse>
    {
        public string Description { get; set; }
        public string SubUserId { get; set; }
    }
    public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationIdResponse>
    {
        private readonly IConversationRepository _conversationRepository;

        public CreateConversationCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<ConversationIdResponse> Handle(CreateConversationCommand request, CancellationToken cancellationToken = default)
        {
            var conversation = new Conversation
            {
                Description = request.Description,
                SubUserId = request.SubUserId,
                DateTimeCreated = DateTime.UtcNow
            };

            await _conversationRepository.AddConversationAsync(conversation, cancellationToken);

            var convId = new ConversationIdResponse()
            {
                ConversationId = conversation.Id.ToString()
            };

            return convId;
        }
    }

    public class ConversationIdResponse
    {
        public string ConversationId { get; set; }
    }
}
