using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using PersonalAudioAssistant.Views;
using MediatR;
using PersonalAudioAssistant.Contracts.SubUser;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class ProgramPageViewModel : ObservableObject, IQueryAttributable
    {
        public Action? RequestScrollToEnd { get; set; }

        private readonly IMediator _mediator;
        private readonly ITextToSpeech _textToSpeech;
        private readonly ISpeechToText _speechToText;
        private readonly ManageCacheData _manageCacheData;
        private readonly MessagesApiClient _messagesApiClient;
        private CancellationTokenSource? _listenCts;
        private int _currentPage = 1;
        private const int PageSize = 10;
        private bool _isLoadingMessages;
        private int _currentIndex = -1;
        private string? _conversationId;
        private string? _subUserId;
        private SubUserResponse? _subUser;
        private ContinueConversation _conversationForContinue;
        private ChatMessage? _selectedMessage;
        private string ConversationIdQueryAttribute;

        [ObservableProperty]
        private bool isCancelContinueAvailable = false;

        [ObservableProperty]
        private List<Locale>? _locales;

        [ObservableProperty]
        private Locale? _locale;

        [ObservableProperty]
        private string? _recognitionText;

        [ObservableProperty]
        private bool _isListening;

        [ObservableProperty]
        private ObservableCollection<ChatMessage> _chatMessages = new();

        [ObservableProperty]
        private bool _canPlayPause;

        [ObservableProperty]
        private bool _canNext;

        [ObservableProperty]
        private bool _canPrevious;

        [ObservableProperty]
        private bool _isAutoPlay;

        [ObservableProperty]
        private bool _allMessagesLoaded = false;

        [ObservableProperty]
        private bool _initialLoadDone = false;

        public bool IsLoadingMessages => _isLoadingMessages;

        public ChatMessage? SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                if (_selectedMessage != value)
                {
                    _selectedMessage = value;
                    OnPropertyChanged();
                    // Consider if any specific action is needed upon selection
                }
            }
        }

        public ProgramPageViewModel(
            ITextToSpeech textToSpeech,
            ISpeechToText speechToText,
            IMediator mediator,
            ManageCacheData manageCacheData,
            MessagesApiClient messagesApiClient)
        {
            _textToSpeech = textToSpeech;
            _speechToText = speechToText;
            _manageCacheData = manageCacheData;
            _mediator = mediator;
            _messagesApiClient = messagesApiClient;
            _conversationForContinue = new();

            Locales = new();
            SetLocalesCommand.Execute(null);
        }

        [RelayCommand]
        private async Task SetLocales()
        {
            Locales = (await _textToSpeech.GetLocalesAsync()).ToList();
            Locale = Locales.FirstOrDefault();
        }

        [RelayCommand(IncludeCancelCommand = true)]
        private async Task Listen(CancellationToken cancellationToken)
        {
            _listenCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var isAuthorized = await _speechToText.RequestPermissions();
            var usersList = await _manageCacheData.GetUsersAsync();

            IsListening = true;

            if (isAuthorized)
            {
                try
                {
                    var chatProgress = new Progress<ChatMessage>(msg => ChatMessages.Add(msg));
                    RecognitionText = string.Empty;
                    RecognitionText = await _speechToText.Listen(
                        CultureInfo.GetCultureInfo(Locale?.Language ?? "en-us"),
                        new Progress<string>(partialText =>
                        {
                            RecognitionText += partialText + " ";
                        }),
                        chatProgress,
                        usersList,
                        _listenCts.Token,
                        () => ChatMessages.Clear(),
                        async () => await RestoreGeneralChatAsync(),
                        ChatMessages.LastOrDefault()?.LastRequestId,
                        _conversationForContinue);
                }
                catch (OperationCanceledException)
                {
                    // Operation was canceled.
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show(cancellationToken);
                }
                finally
                {
                    IsListening = false;
                }
            }
            else
            {
                await Toast.Make("Permission denied").Show(cancellationToken);
                IsListening = false;
            }
        }

        [RelayCommand]
        private void ToggleListen()
        {
            if (IsListening)
            {
                ListenCancelCommand.Execute(null);
            }
            else
            {
                ListenCommand.Execute(null);
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
                if(_conversationForContinue.ConversationId != null)
                {
                    _conversationId = _conversationForContinue.ConversationId;
                }
                else
                {
                    _conversationId = await _manageCacheData.GetСonversationAsync();
                }
                if (string.IsNullOrWhiteSpace(_conversationId))
                    return;

                var messages = await _messagesApiClient.GetMessagesByConversationIdAsync(_conversationId, _currentPage, PageSize);
                if (!messages.Any())
                {
                    AllMessagesLoaded = true;
                    return;
                }

                if (messages.Count < PageSize)
                    AllMessagesLoaded = true;

                var users = await _manageCacheData.GetUsersAsync();

                _subUser = users.Where(u => u.id == messages.FirstOrDefault().subUserId).FirstOrDefault();
                _conversationForContinue.SubUser = _subUser;

                var newSection = new List<ChatMessage>();
                DateTime ? lastDate = null;
                foreach (var msg in messages
                            .Select(msg => new ChatMessage
                {
                    Text = msg.text,
                    UserRole = msg.userRole,
                    DateTimeCreated = msg.dateTimeCreated,
                    URL = msg.audioPath,
                    LastRequestId = msg.lastRequestId,
                    SubUserPhoto = users.FirstOrDefault(u => u.id == msg.subUserId)?.photoPath,
                    ShowDate = false})
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
        private async Task CancelContinueConversation()
        {
            // Скасування слухання
            _listenCts?.Cancel();
            _listenCts?.Dispose();
            _listenCts = null;

            // Очищення полів
            _conversationId = null;
            _subUserId = null;
            _subUser = null;
            _conversationForContinue = new ContinueConversation();
            _currentIndex = -1;
            _currentPage = 1;
            _isLoadingMessages = false;

            // Скидання властивостей
            RecognitionText = null;
            IsListening = false;
            CanPlayPause = false;
            CanNext = false;
            CanPrevious = false;
            IsAutoPlay = false;
            AllMessagesLoaded = false;
            InitialLoadDone = false;
            IsCancelContinueAvailable = false;

            // Очищення колекцій
            ChatMessages.Clear();
            Locales?.Clear();

            SelectedMessage = null;

            await LoadChatMessagesAsync();
            RequestScrollToEnd?.Invoke();
        }

        [RelayCommand]
        private async Task RestoreGeneralChatAsync()
        {
            ChatMessages.Clear();
            _currentPage = 1;
            AllMessagesLoaded = false;
            await LoadChatMessagesAsync();
        }

        public async Task LoadMoreMessagesIfNeeded(double scrollY)
        {
            if (!InitialLoadDone)
                return;
            if (scrollY < 50 && !_isLoadingMessages && !AllMessagesLoaded)
                await LoadChatMessagesAsync();
        }

        public async Task LoadNextPageAsync()
        {
            if (_isLoadingMessages || AllMessagesLoaded)
                return;
            await LoadChatMessagesAsync();
        }

        [RelayCommand]
        private async Task PlaySelectedMessageAsync(ChatMessage? message)
        {
            if (message == null || string.IsNullOrEmpty(message.URL))
                return;

            try
            {
                var mediaElement = ((ProgramPage)Shell.Current.CurrentPage).MediaElement;
                _currentIndex = ChatMessages.IndexOf(message);
                UpdateButtonStates();
                mediaElement.Source = message.URL;

                if (mediaElement.CurrentState == MediaElementState.Playing)
                    mediaElement.Pause();
                else
                    mediaElement.Play();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відтворити голос: {ex.Message}", "OK");
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecutePlayPause))]
        private void PlayPause()
        {
            var media = ((ProgramPage)Shell.Current.CurrentPage).MediaElement;
            if (media.CurrentState == MediaElementState.Playing)
                media.Pause();
            else
                media.Play();
        }

        private async Task PlayByIndexAsync(int index)
        {
            var msg = ChatMessages[index];
            if (msg == null || string.IsNullOrEmpty(msg.URL))
                return;

            var media = ((ProgramPage)Shell.Current.CurrentPage).MediaElement;
            media.Source = msg.URL;
            media.Play();

            SelectedMessage = msg;
            await Task.CompletedTask;
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

        private void UpdateButtonStates()
        {
            CanPlayPause = _currentIndex >= 0;
            CanNext = CanExecuteNext();
            CanPrevious = CanExecutePrevious();

            PlayPauseCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
            PreviousCommand.NotifyCanExecuteChanged();
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("conversationId", out var convId)&&(query.TryGetValue("subUserId", out var subId)))
            {
                _conversationForContinue.ConversationId = convId?.ToString();
                _subUserId = subId?.ToString();
                IsCancelContinueAvailable = true;
            }
        }

        public void OnNavigatedFrom()
        {
            try
            {
                // Скасування слухання
                _listenCts?.Cancel();
                _listenCts?.Dispose();
                _listenCts = null;

                // Очищення полів
                _conversationId = null;
                _subUserId = null;
                _subUser = null;
                _conversationForContinue = new ContinueConversation();
                _currentIndex = -1;
                _currentPage = 1;
                _isLoadingMessages = false;

                // Скидання властивостей
                RecognitionText = null;
                IsListening = false;
                CanPlayPause = false;
                CanNext = false;
                CanPrevious = false;
                IsAutoPlay = false;
                AllMessagesLoaded = false;
                InitialLoadDone = false;
                IsCancelContinueAvailable = false;

                // Очищення колекцій
                ChatMessages.Clear();
                Locales?.Clear();

                SelectedMessage = null;
            }
            catch (Exception ex)
            {
                // Логування або інформування користувача при необхідності
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

    }

    public class ListenButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool isListening && isListening) ? "Стоп" : "Старт";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime DateTimeCreated { get; set; }
        public string? URL { get; set; }
        public string? SubUserPhoto { get; set; }
        public string? LastRequestId { get; set; }
        public bool ShowDate { get; set; }
    }

    public class ContinueConversation
    {
        public string ConversationId { get; set; }
        public SubUserResponse SubUser {get; set; }
    }

    public class BoolToColorConverterChat : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Colors.Blue : Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
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