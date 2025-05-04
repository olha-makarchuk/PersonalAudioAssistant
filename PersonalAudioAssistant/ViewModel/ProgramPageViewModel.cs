using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Apis.Drive.v3.Data;
using MediatR;
using PersonalAudioAssistant.Contracts.Message;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using PersonalAudioAssistant.Views;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class ProgramPageViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly ITextToSpeech textToSpeech;
        private readonly ISpeechToText speechToText;
        private readonly ManageCacheData _manageCacheData;
        private CancellationTokenSource? _listenCts;

        private const int PageSize = 10;
        private int currentPage = 1;
        private bool isLoadingMessages;
        private int _currentIndex = -1;
        private List<SubUserResponse> _subUsers;
        private string? conversationId;

        private ChatMessage _selectedMessage;

        private readonly MessagesApiClient _messagesApiClient;

        [ObservableProperty]
        private List<Locale>? locales;

        [ObservableProperty]
        private Locale? locale;

        [ObservableProperty]
        private string? recognitionText;

        [ObservableProperty]
        private bool isListening;

        [ObservableProperty]
        private ObservableCollection<ChatMessage> chatMessages = new();

        [ObservableProperty]
        private bool canPlayPause;

        [ObservableProperty]
        private bool canNext;

        [ObservableProperty]
        private bool canPrevious;

        [ObservableProperty]
        private bool isAutoPlay;

        public bool IsLoadingMessages
            => isLoadingMessages;

        [ObservableProperty]
        public bool allMessagesLoaded = false;


        public ChatMessage SelectedMessage
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

        [RelayCommand]
        public async Task LoadMore()
        {
            if (!InitialLoadDone)
                return;             // блокуємо доки InitialLoadDone == false
            await LoadInitialChatAsync();
        }


        public ProgramPageViewModel(
            ITextToSpeech textToSpeech,
            ISpeechToText speechToText,
            IMediator mediator,
            ManageCacheData manageCacheData,
            MessagesApiClient messagesApiClient)
        {
            this.textToSpeech = textToSpeech;
            this.speechToText = speechToText;
            _manageCacheData = manageCacheData;
            _mediator = mediator;
            _messagesApiClient = messagesApiClient;

            Locales = new();
            SetLocalesCommand.Execute(null);
        }
        public async Task LoadInitialChatAsync()
        {
            if (isLoadingMessages || AllMessagesLoaded)
                return;

            isLoadingMessages = true;

            try
            {
                conversationId = await _manageCacheData.GetСonversationAsync();
                if (string.IsNullOrWhiteSpace(conversationId))
                    return;

                var messages = await _messagesApiClient.GetMessagesByConversationIdAsync(conversationId, currentPage, PageSize);
                if (!messages.Any())
                    return;

                if (messages.Count < PageSize)
                    AllMessagesLoaded = true;

                var users = await _manageCacheData.GetUsersAsync();

                // Combine existing and new messages
                var combined = ChatMessages
                    .Where(m => !m.ShowDate)
                    .Cast<ChatMessage>()
                    .Concat(messages.Select(msg => new ChatMessage
                    {
                        Text = msg.text,
                        UserRole = msg.userRole,
                        DateTimeCreated = msg.dateTimeCreated,
                        URL = msg.audioPath,
                        LastRequestId = msg.lastRequestId,
                        SubUserPhoto = users.FirstOrDefault(u => u.id == msg.subUserId)?.photoPath,
                        ShowDate = false
                    }))
                    .OrderBy(m => m.DateTimeCreated)
                    .ToList();

                // Clear existing messages and rebuild with date headers
                ChatMessages.Clear();
                DateTime? lastDate = null;

                foreach (var msg in combined)
                {
                    if (!lastDate.HasValue || msg.DateTimeCreated.Date != lastDate.Value)
                    {
                        ChatMessages.Add(new ChatMessage
                        {
                            ShowDate = true,
                            DateTimeCreated = msg.DateTimeCreated
                        });
                        lastDate = msg.DateTimeCreated.Date;
                    }
                    ChatMessages.Add(msg);
                }

                currentPage++;

                if (!InitialLoadDone)
                {
                    InitialLoadDone = true;
                }
            }
            finally
            {
                isLoadingMessages = false;
                //LoadMoreCommand.NotifyCanExecuteChanged();
            }
        }

        

        [RelayCommand]
        public async Task RestoreGeneralChatAsync()
        {
            // Очистимо нинішні повідомлення
            ChatMessages.Clear();

            // Скинемо пагінацію
            currentPage = 1;
            AllMessagesLoaded = false;

            // Завантажимо знову першу сторінку загальних повідомлень
            await LoadInitialChatAsync();
        }

        [ObservableProperty]
        private bool initialLoadDone;

        public async Task LoadMoreMessagesIfNeeded(double scrollY)
        {
            if (!InitialLoadDone)
                return;              // блокуємо до першого LoadInitialChatAsync
            if (scrollY < 50 && !isLoadingMessages && !AllMessagesLoaded) // Додано перевірку isLoadingMessages та allMessagesLoaded
                await LoadInitialChatAsync();
        }

        public async Task LoadNextPageAsync()
        {
            if (isLoadingMessages || AllMessagesLoaded)
                return;

            await LoadInitialChatAsync();  // тут можна перейменувати сам метод у LoadPageAsync
        }
        [RelayCommand]
        async Task SetLocales()
        {
            Locales = (await textToSpeech.GetLocalesAsync()).ToList();
            Locale = Locales.FirstOrDefault();
        }

        [RelayCommand(IncludeCancelCommand = true)]
        async Task Listen(CancellationToken cancellationToken)
        {
            _listenCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var isAuthorized = await speechToText.RequestPermissions();
            var usersList = await _manageCacheData.GetUsersAsync();

            IsListening = true;

            if (isAuthorized)
            {
                try
                {
                    var chatProgress = new Progress<ChatMessage>(msg => ChatMessages.Add(msg));
                    RecognitionText = string.Empty;
                    RecognitionText = await speechToText.Listen(
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
                        ChatMessages.LastOrDefault().LastRequestId);
                }
                catch (OperationCanceledException)
                {
                    // Operation was canceled: do nothing special.
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show(cancellationToken);
                }
                finally
                {
                    // Stop showing the listening indicator.
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
        void ToggleListen()
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
        public async Task PlaySelectedMessageAsync(ChatMessage message)
        {
            try
            {
                var mediaElement = ((ProgramPage)Shell.Current.CurrentPage).MediaElement;
                _currentIndex = ChatMessages.IndexOf(message);
                UpdateButtonStates();
                if (message == null || string.IsNullOrEmpty(message.URL))
                    return;

                mediaElement.Source = message.URL;

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
        public string Text { get; set; }
        public string UserRole { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public string URL { get; set; }
        public string SubUserPhoto { get; set; }
        public string LastRequestId { get; set; }
        public bool ShowDate { get; set; } // Нова властивість
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
        var str = value as string;
        return !string.IsNullOrWhiteSpace(str);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
}
