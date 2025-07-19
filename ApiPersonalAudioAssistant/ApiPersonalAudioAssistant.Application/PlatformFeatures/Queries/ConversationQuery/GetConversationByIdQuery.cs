using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.Conversation;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery
{
    public class GetConversationByIdQuery : IRequest<ConversationsResponse>
    {
        public required string ConversationId { get; set; }

        public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationsResponse>
        {
            private readonly IConversationRepository _conversationRepository;

            public GetConversationByIdQueryHandler(IConversationRepository conversationRepository)
            {
                _conversationRepository = conversationRepository;
            }

            public async Task<ConversationsResponse> Handle(GetConversationByIdQuery query, CancellationToken cancellationToken)
            {
                var conversation = await _conversationRepository.GetConversationByIdAsync(query.ConversationId, cancellationToken);

                if (conversation == null)
                {
                    throw new Exception("Conversation not found");
                }

                var response = new ConversationsResponse
                {
                    IdConversation = conversation.Id.ToString(),
                    Description = conversation.Description,
                    SubUserId = conversation.SubUserId
                };

                return response;
            }
        }
    }
}
