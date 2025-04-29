using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using System.Collections.ObjectModel;
using PersonalAudioAssistant.Contracts.Message;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Input;
using PersonalAudioAssistant.Views.Users;
using PersonalAudioAssistant.Views.History;
using PersonalAudioAssistant.Services.Api;

namespace PersonalAudioAssistant.ViewModel.History
{
    public partial class MessagesViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IMediator _mediator;
        private readonly MessagesApiClient _messagesApiClient;
        private string ConversationIdQueryAttribute;

        [ObservableProperty]
        private ObservableCollection<MessageResponse> messages;

        [ObservableProperty]
        private bool isBusy;

        private MessageResponse _selectedMessage;

        private int _currentPage = 1;
        private bool _isLoadingMore;
        private bool _hasMoreItems = true;
        private const int PageSize = 10;

        public MessagesViewModel(IMediator mediator, MessagesApiClient messagesApiClient)
        {
            _mediator = mediator;
            _messagesApiClient = messagesApiClient;
            messages = new ObservableCollection<MessageResponse>();
        }

        public MessageResponse SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                if (_selectedMessage != value)
                {
                    _selectedMessage = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        //
                    }
                }
            }
        }

        public Command LoadMoreCommand => new Command(async () =>
        {
            if (_hasMoreItems && !_isLoadingMore)
            {
                await LoadMessagesAsync(isLoadMore: true);
            }
        });

        public async Task LoadMessagesAsync(bool isLoadMore = false)
        {
            if (IsBusy || _isLoadingMore)
                return;

            if (!isLoadMore)
            {
                IsBusy = true;
                _currentPage = 1;
                _hasMoreItems = true;
            }
            else
            {
                _isLoadingMore = true;
            }

            try
            {
                var messagesList = await _messagesApiClient.GetMessageByConversationIdAsync(ConversationIdQueryAttribute, _currentPage, PageSize);

                if (isLoadMore)
                {
                    foreach (var message in messagesList)
                    {
                        Messages.Add(message);
                    }
                }
                else
                {
                    Messages = messagesList.ToObservableCollection();
                }

                if (messagesList.Count < PageSize)
                {
                    _hasMoreItems = false;
                }
                else
                {
                    _currentPage++;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося завантажити дані: {ex.Message}", "OK");
            }
            finally
            {
                if (isLoadMore)
                    _isLoadingMore = false;
                else
                    IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task PlaySelectedMessageAsync(MessageResponse message)
        {
            try
            {
                var mediaElement = ((MessagesPage)Shell.Current.CurrentPage).MediaElement;

                if (message == null || string.IsNullOrEmpty(message.audioPath))
                    return;

                mediaElement.Source = message.audioPath;

                if (mediaElement.CurrentState == CommunityToolkit.Maui.Core.Primitives.MediaElementState.Playing)
                    mediaElement.Pause();
                else
                    mediaElement.Play();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відтворити голос: {ex.Message}", "OK");
            }
        }


        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("conversationId"))
            {
                ConversationIdQueryAttribute = query["conversationId"]?.ToString();
            }
        }

        [RelayCommand]
        public void OnNavigatedFrom()
        {
            SelectedMessage = null;
            Messages.Clear();
        }
    }
}
