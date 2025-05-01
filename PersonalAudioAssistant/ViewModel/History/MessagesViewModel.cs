using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using System.Collections.ObjectModel;
using PersonalAudioAssistant.Contracts.Message;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Input;
using PersonalAudioAssistant.Views.Users;
using PersonalAudioAssistant.Views.History;
using PersonalAudioAssistant.Services.Api;
using CommunityToolkit.Maui.Core.Primitives;
using Android.Content;
using System.Globalization;

namespace PersonalAudioAssistant.ViewModel.History
{
    public partial class MessagesViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IMediator _mediator;
        private readonly MessagesApiClient _messagesApiClient;
        private string ConversationIdQueryAttribute;
        private string PreviewId;

        [ObservableProperty]
        private ObservableCollection<MessageResponse> messages;

        [ObservableProperty]
        private bool isBusy;

        private MessageResponse _selectedMessage;

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
                var messagesList = await _messagesApiClient.GetMessagesByConversationIdAsync(ConversationIdQueryAttribute, _currentPage, PageSize);

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
                if (!isLoadMore && Messages.Any())
                {
                    _currentIndex = 0;
                    UpdateButtonStates();

                    if (IsAutoPlay)
                    {
                        // Якщо вмикнено автоплей — запустити перше повідомлення
                        await PlayByIndexAsync(_currentIndex);
                    }
                }
                PreviewId = Messages.First().lastRequestId;
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
            Messages.Clear();
            PreviewId = null;
            IsAutoPlay = false;
        }


        [RelayCommand]
        public void ContinueConversationCommand()
        {
            //PreviewId
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
            if (_currentIndex < Messages.Count - 1)
            {
                _currentIndex++;
                await PlayByIndexAsync(_currentIndex);
                UpdateButtonStates();
            }
        }

        private bool CanExecuteNext() => _currentIndex < Messages?.Count - 1;

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
            var msg = Messages[index];
            if (msg == null || string.IsNullOrEmpty(msg.audioPath))
                return;

            var media = ((MessagesPage)Shell.Current.CurrentPage).MediaElement;
            media.Source = msg.audioPath;
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

}
