using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.Message;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.MessageQuery
{
    public class GetMessagesByConversationIdQuery : IRequest<List<MessageResponse>>
    {
        public required string ConversationId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public class GetMessagesByConversationIdQueryHandler : IRequestHandler<GetMessagesByConversationIdQuery, List<MessageResponse>>
        {
            private readonly IMessageRepository _messageRepository;

            public GetMessagesByConversationIdQueryHandler(IMessageRepository messageRepository)
            {
                _messageRepository = messageRepository;
            }

            public async Task<List<MessageResponse>> Handle(GetMessagesByConversationIdQuery query, CancellationToken cancellationToken)
            {
                var messages = await _messageRepository.GetMessagesByConversationIdPaginatorAsync(
                    query.ConversationId,
                    query.PageNumber,
                    query.PageSize,
                    cancellationToken);

                if (messages == null)
                {
                    throw new Exception("Messages not found");
                }

                var responseList = messages.Select(mes => new MessageResponse
                {
                    MessageId = mes.Id.ToString(),
                    ConversationId = mes.ConversationId.ToString(),
                    Text = mes.Text,
                    UserRole = mes.UserRole,
                    AudioPath = mes.AudioPath
                }).ToList();

                return responseList;
            }
        }
    }
}
