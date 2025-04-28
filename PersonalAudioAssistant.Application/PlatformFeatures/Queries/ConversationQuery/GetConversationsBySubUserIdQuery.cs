using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentHistory;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Contracts.Conversation;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery
{
    public class GetConversationsBySubUserIdQuery : IRequest<List<AllConversationsResponse>>
    {
        public required string SubUserId { get; set; }
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 1;  

        public class GetConversationsBySubUserIdQueryHandler : IRequestHandler<GetConversationsBySubUserIdQuery, List<AllConversationsResponse>>
        {
            private readonly IConversationRepository _conversationRepository;
            private readonly IMessageRepository _messageRepository;
            private readonly IMediator _mediator;

            public GetConversationsBySubUserIdQueryHandler(IConversationRepository conversationRepository, IMessageRepository messageRepository, IMediator mediator)
            {
                _conversationRepository = conversationRepository;
                _messageRepository = messageRepository;
                _mediator = mediator;
            }

            public async Task<List<AllConversationsResponse>> Handle(GetConversationsBySubUserIdQuery query, CancellationToken cancellationToken)
            {
                var apiGPT = new ApiClientGPT();

                var conversations = await _conversationRepository.GetConversationsByUserIdPaginatorAsync(
                    query.SubUserId,
                    query.PageNumber,
                    query.PageSize,
                    cancellationToken
                );

                if (conversations == null)
                {
                    throw new Exception("Conversation not found");
                }

                var responseList = new List<AllConversationsResponse>();

                foreach (var conv in conversations)
                {
                    if (string.IsNullOrWhiteSpace(conv.Description))
                    {
                        var message = await _messageRepository.GetLastMessageByConversationIdAsync(conv.Id.ToString(), cancellationToken);

                        ApiClientGptResponse descriptionGpt = await apiGPT.ContinueChatAsync(
                            "На основі розмови напиши короткий заголовок, який підсумовує основну тему",
                            message.LastRequestId
                        );

                        var updateCommand = new UpdateConversationCommand
                        {
                            ConversationId = conv.Id.ToString(),
                            Description = descriptionGpt.text
                        };

                        await _mediator.Send(updateCommand, cancellationToken);
                    }

                    responseList.Add(new AllConversationsResponse
                    {
                        IdConversation = conv.Id.ToString(),
                        Description = conv.Description,
                        DateTimeCreated = conv.DateTimeCreated
                    });
                }
                return responseList
                        .OrderByDescending(x => x.DateTimeCreated)
                        .ToList();
            }
        }
    }
}
