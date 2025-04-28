using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;

namespace PersonalAudioAssistant.ViewModel.History
{
    public partial class MessagesViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IMediator _mediator;
        private string ConversationIdQueryAttribute;

        public MessagesViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task LoadMessagesAsync()
        {
            // Initialization logic here
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("conversationId"))
            {
                ConversationIdQueryAttribute = query["conversationId"]?.ToString();
            }
        }
    }
}
