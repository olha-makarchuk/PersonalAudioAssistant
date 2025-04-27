

using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.Message;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.MessageQuery
{
    public class GetLastMessageByConversationIdQuery : IRequest
    {
        public required string ConversationId { get; set; }

        public class GetLastMessageByConversationIdQueryHandler : IRequestHandler<GetLastMessageByConversationIdQuery>
        {
            private readonly IMessageRepository _messageRepository;

            public GetLastMessageByConversationIdQueryHandler(IMessageRepository messageRepository)
            {
                _messageRepository = messageRepository;
            }

            public async Task Handle(GetLastMessageByConversationIdQuery query, CancellationToken cancellationToken)
            {
                var messages = await _messageRepository.GetLastMessageByConversationIdAsync(query.ConversationId, cancellationToken);
                if (messages == null)
                {
                    throw new Exception("Payment not found");
                }
            }
        }
    }
}
