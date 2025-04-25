using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.Conversation;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery
{
    public class GetConversationsByIdQuery : IRequest<ConversationsResponse>
    {
        public required string SubUserId { get; set; }

        public class GetConversationsByIdQueryHandler : IRequestHandler<GetConversationsByIdQuery, ConversationsResponse>
        {
            private readonly IConversationRepository _conversationRepository;

            public GetConversationsByIdQueryHandler(IConversationRepository conversationRepository)
            {
                _conversationRepository = conversationRepository;
            }

            public async Task<ConversationsResponse> Handle(GetConversationsByIdQuery query, CancellationToken cancellationToken)
            {
                var conversations = await _conversationRepository.GetConversationByIdAsync(query.SubUserId, cancellationToken);

                if (conversations == null)
                {
                    throw new Exception("Conversation not found");
                }

                var response = new ConversationsResponse
                {
                    IdConversation = conversations.Id.ToString(),
                    Description = conversations.Description
                };

                return response;
            }
        }
    }
}
