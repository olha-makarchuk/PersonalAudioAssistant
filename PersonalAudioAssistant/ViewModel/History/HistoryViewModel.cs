using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Contracts.Conversation;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.History
{
    public partial class HistoryViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IMediator _mediator;
        private string _userIdQueryAttribute;
        private readonly ManageCacheData _manageCacheData;
        private readonly ConversationApiClient _conversationApiClient;

        [ObservableProperty]
        private ObservableCollection<ConversationGroup> conversationsGrouped;

        [ObservableProperty]
        private string subUserId;

        [ObservableProperty]
        private ObservableCollection<AllConversationsResponse> conversations;

        [ObservableProperty]
        private bool isBusy;

        private AllConversationsResponse _selectedConversation;

        private int _currentPage = 1;
        private bool _isLoadingMore;
        private bool _hasMoreItems = true;
        private const int PageSize = 10;

        public HistoryViewModel(IMediator mediator, ManageCacheData manageCacheData, ConversationApiClient conversationApiClient)
        {
            _mediator = mediator;
            _manageCacheData = manageCacheData;
            _conversationApiClient = conversationApiClient;

            Conversations = new ObservableCollection<AllConversationsResponse>();
        }

        public AllConversationsResponse SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                if (_selectedConversation != value)
                {
                    _selectedConversation = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        OpenConversationDetails(value);
                    }
                }
            }
        }

        private async void OpenConversationDetails(AllConversationsResponse conversation)
        {
            // Перехід до сторінки деталей розмови
            await Shell.Current.GoToAsync($"/MessagesPage?conversationId={conversation.IdConversation}&subUserId={SubUserId}");
        }

        public async Task LoadHistoryAsync(bool isLoadMore = false)
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
                var conversationsList = await _conversationApiClient.GetConversationsBySubUserIdAsync(_userIdQueryAttribute, _currentPage, PageSize);

                if (conversationsList.Count != 0)
                {
                    SubUserId = conversationsList.FirstOrDefault().SubUserId;
                }

                if (isLoadMore)
                {
                    foreach (var conversation in conversationsList)
                    {
                        Conversations.Add(conversation);
                    }
                }
                else
                {
                    Conversations = conversationsList.ToObservableCollection();
                }

                if (conversationsList.Count < PageSize)
                {
                    _hasMoreItems = false;
                }
                else
                {
                    _currentPage++;
                }

                // Після того, як у вас є повний список (Conversations або новий список із сервера)
                var all = Conversations.Cast<AllConversationsResponse>()
                            .Union(conversationsList) // якщо це load more
                            .ToList();

                var grouped = all
                    .OrderByDescending(c => c.DateTimeCreated)
                    .GroupBy(c => c.DateTimeCreated.Date)
                    .Select(g => new ConversationGroup(g.Key, g))
                    .ToObservableCollection();

                ConversationsGrouped = grouped;
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

        public Command LoadMoreCommand => new Command(async () =>
        {
            if (_hasMoreItems && !_isLoadingMore)
            {
                await LoadHistoryAsync(isLoadMore: true);
            }
        });

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("userId"))
            {
                _userIdQueryAttribute = query["userId"]?.ToString();

                await LoadHistoryAsync();
            }
        }

        [RelayCommand]
        public void OnNavigatedFrom()
        {
            SelectedConversation = null;
            Conversations.Clear();
        }
    }

    public class ConversationGroup : ObservableCollection<AllConversationsResponse>
    {
        public DateTime Date { get; }
        public string DateDisplay => Date.ToString("dd.MM.yyyy");

        public ConversationGroup(DateTime date, IEnumerable<AllConversationsResponse> items)
            : base(items)
        {
            Date = date;
        }
    }
}
