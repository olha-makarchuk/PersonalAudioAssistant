using PersonalAudioAssistant.Contracts.Message;

namespace PersonalAudioAssistant.Services.Api
{
    public class MessagesApiClient : BaseApiClient
    {
        public MessagesApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<MessageResponse> CreateMessageAsync(CreateMessageCommand createMessageCommand)
        {
            var url = $"{BaseUrl}Message";

            var request = new
            {
                ConversationId = createMessageCommand.ConversationId,
                Text = createMessageCommand.Text,
                UserRole = createMessageCommand.UserRole,
                Audio = createMessageCommand.Audio,
                LastRequestId = createMessageCommand.LastRequestId
            };

            var message = await PostAsync<object, MessageResponse>(url, request);
            return message;
        }

        public async Task DeleteMessagesByConversationIdAsync(string idConversation)
        {
            var url = $"{BaseUrl}Message";

            var request = new
            {
                IdConversation = idConversation
            };

            await DeleteAsync(url, request);
        }

        public async Task<List<MessageResponse>> GetMessagesByConversationIdAsync(string conversationId, int pageNumber, int pageSize)
        {
            var url = $"{BaseUrl}Message/byconversationid";

            var request = new
            {
                ConversationId = conversationId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var voice = await PostAsync<object, List<MessageResponse>>(url, request);

            return voice;
        }
    }

    public class CreateMessageCommand
    {
        public string ConversationId { get; set; }
        public string Text { get; set; }
        public string UserRole { get; set; }
        public byte[] Audio { get; set; }
        public string? LastRequestId { get; set; }
    }
}
