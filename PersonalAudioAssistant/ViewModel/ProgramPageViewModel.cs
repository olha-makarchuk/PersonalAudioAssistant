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
using System.Threading;
using PersonalAudioAssistant.Model;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class ProgramPageViewModel : ObservableObject, IQueryAttributable
    {
        public Action? RequestScrollToEnd { get; set; }
        [ObservableProperty]
        private bool _isPrivateConversation = false;
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
        private bool CanExecutePrevious() => _currentIndex > 0;
        private bool CanExecuteNext() => _currentIndex < ChatMessages?.Count - 1;
        private bool CanExecutePlayPause() => _currentIndex >= 0;
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
            var chatProgress = new Progress<ChatMessage>(msg => ChatMessages.Add(msg));

            IsListening = true;

            if (_conversationForContinue.ConversationId == null)
            {
                if (isAuthorized)
                {
                    try
                    {
                        var lastRequestId = ChatMessages
                        .AsEnumerable()
                        .Reverse()
                        .FirstOrDefault(x => !string.IsNullOrEmpty(x.LastRequestId))
                        ?.LastRequestId;
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
                            () => ClearOnly(),
                            async () => await RestoreGeneralChatAsync(),
                            lastRequestId);
                    }
                    catch (OperationCanceledException)
                    {
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
            else
            {
                try
                {
                    var lastRequestId = ChatMessages
                    .AsEnumerable() 
                    .Reverse() 
                    .FirstOrDefault(x => !string.IsNullOrEmpty(x.LastRequestId))
                    ?.LastRequestId;

                    RecognitionText = string.Empty;
                    RecognitionText = await _speechToText.ContinueListen(
                        new Progress<string>(partialText =>
                        {
                            RecognitionText += partialText + " ";
                        }),
                        chatProgress,
                        _listenCts.Token,
                        async () => await CleanAll(),
                        async () => await RestoreGeneralChatAsync(),
                        lastRequestId,
                        _conversationForContinue);
                }
                catch (OperationCanceledException)
                {
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
        }

        private void ClearOnly()
        {
            ChatMessages.Clear();
            IsPrivateConversation = true;
            _currentPage = 1;
            AllMessagesLoaded = false;
            InitialLoadDone = false;
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
                IsListening = true; 
            }
        }

        [RelayCommand]
        private async Task LoadMore()
        {
            if (!InitialLoadDone)return;
            await LoadChatMessagesAsync();
        }

        public async Task LoadChatMessagesAsync()
        {
            if (_isLoadingMessages || AllMessagesLoaded)return;

            _isLoadingMessages = true;

            try
            {
                if(_conversationForContinue.ConversationId != null)
                {
                    _conversationId = _conversationForContinue.ConversationId;
                }
                else
                {
                    _conversationId = await _manageCacheData.GetConversationAsync();
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

                if (_subUserId != null)
                {
                    _subUser = users.Where(u => u.id == _subUserId).FirstOrDefault();
                    _conversationForContinue.SubUser = _subUser;
                }
                else
                {
                    _subUser = users.Where(u => u.id == messages.FirstOrDefault().subUserId).FirstOrDefault();
                }

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

        public async Task LoadMoreMessagesIfNeeded(double scrollY)
        {
            if (!InitialLoadDone)return;
            if (scrollY < 50 && !_isLoadingMessages && !AllMessagesLoaded)
                await LoadChatMessagesAsync();
        }

        public async Task LoadNextPageAsync()
        {
            if (_isLoadingMessages || AllMessagesLoaded)return;
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

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("conversationId", out var convId)&&(query.TryGetValue("subUserId", out var subId)))
            {
                _conversationForContinue.ConversationId = convId?.ToString();
                _subUserId = subId?.ToString();
                IsCancelContinueAvailable = true;
            }
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

        [RelayCommand]
        private async Task CancelContinueConversation()
        {
            await CleanAll();
        }

        [RelayCommand]
        private async Task RestoreGeneralChatAsync()
        {
            ChatMessages.Clear();
            IsPrivateConversation = false;
            _currentPage = 1;
            InitialLoadDone = false;
            AllMessagesLoaded = false;

            // Скидаємо весь локальний стан
            await CleanAll();

            // Завантажуємо першу сторінку загальних повідомлень
            await LoadChatMessagesAsync();

            // Прокрутка до кінця
            RequestScrollToEnd?.Invoke();

        }

        public async Task CancelCleanContinueConversation()
        {
            await CleanAll();
            await LoadChatMessagesAsync();
            RequestScrollToEnd?.Invoke();
        }

        public async Task CleanAll()
        {
            // Скасування слухання
            _listenCts?.Cancel();


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

        public async void OnNavigatedFrom()
        {
            try
            {
                await CleanAll();
                _listenCts?.Dispose();
                _listenCts = null;
            }
            catch (Exception ex)
            {
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