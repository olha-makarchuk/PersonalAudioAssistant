using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PersonalAudioAssistant.Views.History;
using PersonalAudioAssistant.Services.Api;
using CommunityToolkit.Maui.Core.Primitives;
using System.Globalization;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Model.Payment;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.Contracts.Conversation;
using static Android.Provider.Telephony.Sms;

namespace PersonalAudioAssistant.ViewModel.History
{
    public partial class MessagesViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IMediator _mediator;
        private readonly MessagesApiClient _messagesApiClient;
        private readonly ConversationApiClient _conversationApiClient;
        private string ConversationIdQueryAttribute;
        private string SubUserIdQueryAttribute;
        private string PreviewId;
        private readonly ManageCacheData _manageCacheData;
        public Action? RequestScrollToEnd { get; set; }

        [ObservableProperty]
        private bool isBusy;

        private ChatMessage _selectedMessage;

        private int _currentPage = 1;
        private bool _isLoadingMore;
        private bool _hasMoreItems = true;
        private const int PageSize = 10;
        private int _currentIndex = -1;

        [ObservableProperty]
        private bool canPlayPause;

        [ObservableProperty]
        private bool canNext;

        [ObservableProperty]
        private bool canPrevious;

        [ObservableProperty]
        private bool isAutoPlay;

        [ObservableProperty]
        private ObservableCollection<ChatMessage> _chatMessages = new();

        [ObservableProperty]
        private bool _allMessagesLoaded = false;

        [ObservableProperty]
        private bool _initialLoadDone = false;

        public bool IsLoadingMessages => _isLoadingMessages;

        private bool _isLoadingMessages;


        public MessagesViewModel(IMediator mediator, MessagesApiClient messagesApiClient, ManageCacheData manageCacheData, ConversationApiClient conversationApiClient)
        {
            _mediator = mediator;
            _messagesApiClient = messagesApiClient;
            _manageCacheData = manageCacheData;
            _conversationApiClient = conversationApiClient;
        }

        public ChatMessage? SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                if (_selectedMessage != value)
                {
                    _selectedMessage = value;
                    OnPropertyChanged();
                }
            }
        }


        [RelayCommand]
        private async Task LoadMore()
        {
            if (!InitialLoadDone)
                return;
            await LoadChatMessagesAsync();
        }


        public async Task LoadChatMessagesAsync()
        {
            if (_isLoadingMessages || AllMessagesLoaded)
                return;

            _isLoadingMessages = true;

            try
            {
                var messages = await _messagesApiClient.GetMessagesByConversationIdAsync(ConversationIdQueryAttribute, _currentPage, PageSize);
                
                if (!messages.Any())
                {
                    AllMessagesLoaded = true;
                    return;
                }

                if (messages.Count < PageSize)
                    AllMessagesLoaded = true;

                var users = await _manageCacheData.GetUsersAsync();

                var newSection = new List<ChatMessage>();
                DateTime? lastDate = null;
                foreach (var msg in messages
                            .Select(msg => new ChatMessage
                            {
                                Text = msg.text,
                                UserRole = msg.userRole,
                                DateTimeCreated = msg.dateTimeCreated,
                                URL = msg.audioPath,
                                LastRequestId = msg.lastRequestId,
                                SubUserPhoto = users.FirstOrDefault(u => u.id == msg.subUserId)?.photoPath,
                                ShowDate = false
                            })
                        .OrderBy(m => m.DateTimeCreated))
                {
                    if (!lastDate.HasValue || msg.DateTimeCreated.Date != lastDate.Value.Date)
                    {
                        newSection.Add(new ChatMessage { ShowDate = true, DateTimeCreated = msg.DateTimeCreated });
                        lastDate = msg.DateTimeCreated.Date;
                    }
                    newSection.Add(msg);
                }
                if (!InitialLoadDone)
                {
                    foreach (var msg in newSection)
                        ChatMessages.Add(msg);
                    InitialLoadDone = true;
                }
                else
                {
                    for (int i = newSection.Count - 1; i >= 0; i--)
                        ChatMessages.Insert(0, newSection[i]);
                }

                _currentPage++;

                if (!InitialLoadDone)
                {
                    InitialLoadDone = true;
                }
            }
            finally
            {
                _isLoadingMessages = false;
            }
        }

        [RelayCommand]
        public async Task PlaySelectedMessageAsync(ChatMessage message)
        {
            try
            {
                if (message == null || string.IsNullOrEmpty(message.URL))
                    return;

                // sync our internal pointer to whichever message was tapped
                _currentIndex = ChatMessages.IndexOf(message);

                var mediaElement = ((MessagesPage)Shell.Current.CurrentPage).MediaElement;
                mediaElement.Source = message.URL;

                if (mediaElement.CurrentState == MediaElementState.Playing)
                    mediaElement.Pause();
                else
                    mediaElement.Play();

                // now enable/disable Prev/Next correctly
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відтворити голос: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        private async Task DeleteConversationAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Видалити?",
                $"Ви дійсно хочете видалити розмову?",
                "Так", "Ні");

            if (!confirm)
                return;

            try
            {
                IsBusy = true;
                await _conversationApiClient.DeleteConversationAsync(ConversationIdQueryAttribute);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося видалити: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadNextPageAsync()
        {
            if (_isLoadingMessages || AllMessagesLoaded)
                return;
            await LoadChatMessagesAsync();
        }
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("conversationId") && query.ContainsKey("subUserId"))
            {
                ConversationIdQueryAttribute = query["conversationId"]?.ToString();
                SubUserIdQueryAttribute = query["subUserId"]?.ToString();
            }
        }

        public void OnNavigatedFrom()
        {
            try
            {
                var media = ((MessagesPage)Shell.Current.CurrentPage).MediaElement;
                media.Stop();
            }
            catch (Exception)
            {
            }

            SelectedMessage = null;
            ChatMessages.Clear();
            PreviewId = null;
            IsAutoPlay = false;
            AllMessagesLoaded = false;
            InitialLoadDone = false;
            _isLoadingMessages = false;
            _currentPage = 1;
            _isLoadingMore = false;
            _hasMoreItems = true;
            _currentIndex = -1;
        }

        [RelayCommand]
        public async void ContinueConversation()
        {
            await Shell.Current.GoToAsync($"//ProgramPage?conversationId={ConversationIdQueryAttribute}&subUserId={SubUserIdQueryAttribute}");
        }


        [RelayCommand(CanExecute = nameof(CanExecutePlayPause))]
        private void PlayPause()
        {
            var media = ((MessagesPage)Shell.Current.CurrentPage).MediaElement;
            if (media.CurrentState == MediaElementState.Playing)
                media.Pause();
            else
                media.Play();
        }

        private bool CanExecutePlayPause() => _currentIndex >= 0;

        [RelayCommand(CanExecute = nameof(CanExecuteNext))]
        public async Task NextAsync()
        {
            if (_currentIndex < ChatMessages.Count - 1)
            {
                _currentIndex++;
                await PlayByIndexAsync(_currentIndex);
                UpdateButtonStates();
            }
        }

        private bool CanExecuteNext() => _currentIndex < ChatMessages?.Count - 1;

        [RelayCommand(CanExecute = nameof(CanExecutePrevious))]
        private async Task PreviousAsync()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                await PlayByIndexAsync(_currentIndex);
                UpdateButtonStates();
            }
        }

        private bool CanExecutePrevious() => _currentIndex > 0;

        private async Task PlayByIndexAsync(int index)
        {
            var msg = ChatMessages[index];
            if (msg == null || string.IsNullOrEmpty(msg.URL))
                return;

            var media = ((MessagesPage)Shell.Current.CurrentPage).MediaElement;
            media.Source = msg.URL;
            media.Play();

            SelectedMessage = msg;
            await Task.CompletedTask;
        }

        private void UpdateButtonStates()
        {
            CanPlayPause = _currentIndex >= 0;
            CanNext = CanExecuteNext();
            CanPrevious = CanExecutePrevious();

            PlayPauseCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
            PreviousCommand.NotifyCanExecuteChanged();
        }
    }

    public class BoolToPlayPauseTextConverter : IValueConverter
    {
        // value буде вашим булевим полем з VM (наприклад IsPlaying або IsAutoPlay)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "Пауза" : "Відтворити";
            return "Відтворити";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            !(bool)value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            !(bool)value;
    }

    public class NullOrEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
