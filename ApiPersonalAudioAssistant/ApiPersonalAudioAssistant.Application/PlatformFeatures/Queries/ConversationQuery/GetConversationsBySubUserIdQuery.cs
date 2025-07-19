using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands;
using ApiPersonalAudioAssistant.Application.Services;
using ApiPersonalAudioAssistant.Contracts.Conversation;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery
{
    public class GetConversationsBySubUserIdQuery : IRequest<List<AllConversationsResponse>>
    {
        public required string SubUserId { get; set; }
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 10;  

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
                var responseList = new List<AllConversationsResponse>();

                int currentPage = query.PageNumber;
                int conversationsFetched = 0;

                while (responseList.Count < query.PageSize)
                {
                    var conversations = await _conversationRepository.GetConversationsByUserIdPaginatorAsync(
                        query.SubUserId,
                        currentPage,
                        query.PageSize,
                        cancellationToken
                    );

                    if (conversations == null || !conversations.Any())
                        break; // немає більше розмов у базі

                    foreach (var conv in conversations)
                    {
                        var message = await _messageRepository.GetLastMessageByConversationIdAsync(conv.Id.ToString(), cancellationToken);

                        if (message == null)
                        {
                            // Видаляємо розмову без повідомлень
                            await _conversationRepository.DeleteConversationAsync(conv, cancellationToken);
                            continue;
                        }

                        // Якщо опис порожній — генеруємо за допомогою GPT
                        if (string.IsNullOrWhiteSpace(conv.Description))
                        {
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
                            conv.Description = descriptionGpt.text;
                        }

                        responseList.Add(new AllConversationsResponse
                        {
                            IdConversation = conv.Id.ToString(),
                            Description = conv.Description,
                            SubUserId = conv.SubUserId.ToString(),
                            DateTimeCreated = conv.DateTimeCreated
                        });

                        if (responseList.Count == query.PageSize)
                            break; // вже набрали потрібну кількість
                    }

                    currentPage++; // переходимо до наступної сторінки
                }

                return responseList
                    .OrderByDescending(x => x.DateTimeCreated)
                    .ToList();
            }
        }
    }
}
