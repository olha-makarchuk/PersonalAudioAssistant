using PersonalAudioAssistant.Contracts.Conversation;

namespace PersonalAudioAssistant.Services.Api
{
    public class ConversationApiClient : BaseApiClient
    {
        public ConversationApiClient(HttpClient httpClient) : base(httpClient) { }


        public async Task<string> CreateConversationAsync(string description, string subUserId)
        {
            var url = $"{BaseUrl}Conversation";

            var request = new
            {
                Description = description,
                SubUserId = subUserId
            };

            var conversationId = await PostAsync<object, ConversationIdResponse>(url, request);

            return conversationId.ConversationId;
        }

        public async Task UpdateConversationAsync(string conversationId, string description)
        {
            var url = $"{BaseUrl}Conversation";

            var request = new
            {
                ConversationId = conversationId,
                Description = description
            };

            await PutAsync<object, string>(url, request);
        }

        public async Task DeleteConversationAsync(string idConversation)
        {
            var url = $"{BaseUrl}Conversation";

            var request = new
            {
                IdConversation = idConversation
            };

            await DeleteAsync(url, request);
        }

        public async Task GetConversationByIdAsync(string idConversation, string description)
        {
            var url = $"{BaseUrl}Conversation/byid";

            var request = new
            {
                IdConversation = idConversation,
                Description = description
            };

            await PostAsync<object, string>(url, request);
        }

        public async Task<List<AllConversationsResponse>> GetConversationsBySubUserIdAsync(string subUserId, int pageNumber, int pageSize)
        {
            var url = $"{BaseUrl}Conversation/bysubuser";

            var request = new
            {
                SubUserId = subUserId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var voice = await PostAsync<object, List<AllConversationsResponse>>(url, request);

            return voice;
        }
    }
    public class ConversationIdResponse
    {
        public string ConversationId { get; set; }
    }
}
