using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery;
using PersonalAudioAssistant.Contracts.Conversation;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Services;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.History
{
    public partial class HistoryViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IMediator _mediator;
        private string _userIdQueryAttribute;
        private readonly ManageCacheData _manageCacheData;

        [ObservableProperty]
        private SubUserResponse user;

        [ObservableProperty]
        private ObservableCollection<AllConversationsResponse> conversations;

        [ObservableProperty]
        private bool isBusy;

        private int _currentPage = 1;
        private bool _isLoadingMore;
        private bool _hasMoreItems = true;
        private const int PageSize = 1;

        public HistoryViewModel(IMediator mediator, ManageCacheData manageCacheData)
        {
            _mediator = mediator;
            _manageCacheData = manageCacheData;

            Conversations = new ObservableCollection<AllConversationsResponse>();
        }

        private AllConversationsResponse _selectedConversation;
        public AllConversationsResponse SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                if (_selectedConversation != value)
                {
                    _selectedConversation = value;
                    OnPropertyChanged();

                    // Тут можна обробити вибір (наприклад, навігація на нову сторінку)
                    if (value != null)
                    {
                        // Наприклад, відкрити нову сторінку з деталями
                        OpenConversationDetails(value);
                    }
                }
            }
        }

        private async void OpenConversationDetails(AllConversationsResponse conversation)
        {
            // Реалізація навігації або показу діалогу
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
                if (User == null)
                {
                    var cached = await _manageCacheData.GetUsersAsync();
                    User = cached.FirstOrDefault(x => x.Id == _userIdQueryAttribute);
                }

                var command = new GetConversationsBySubUserIdQuery()
                {
                    SubUserId = _userIdQueryAttribute,
                    PageNumber = _currentPage,
                    PageSize = PageSize
                };

                var conversationsList = await _mediator.Send(command);

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
    }
}
