using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentHistory;
using PersonalAudioAssistant.Contracts.Conversation;
using PersonalAudioAssistant.Contracts.PaymentHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery
{
    public class GetConversationsBySubUserIdQuery : IRequest<List<AllConversationsResponse>>
    {
        public required string SubUserId { get; set; }

        public class GetConversationsBySubUserIdQueryHandler : IRequestHandler<GetConversationsBySubUserIdQuery, List<AllConversationsResponse>>
        {
            private readonly IConversationRepository _conversationRepository;

            public GetConversationsBySubUserIdQueryHandler(IConversationRepository conversationRepository)
            {
                _conversationRepository = conversationRepository;
            }

            public async Task<List<AllConversationsResponse>> Handle(GetConversationsBySubUserIdQuery query, CancellationToken cancellationToken)
            {
                var conversations = await _conversationRepository.GetConversationsByUserIdAsync(query.SubUserId, cancellationToken);

                if (conversations == null)
                {
                    throw new Exception("Conversation not found");
                }

                var responseList = conversations.Select(conv => new AllConversationsResponse
                {
                    IdConversation = conv.Id.ToString(),
                    Description = conv.Description
                }).ToList();

                return responseList;
            }
        }
    }
}
