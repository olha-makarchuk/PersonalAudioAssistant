using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using IntelliJ.Lang.Annotations;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery;
using PersonalAudioAssistant.Contracts.Conversation;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Services;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel
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

        public HistoryViewModel(IMediator mediator, ManageCacheData manageCacheData)
        {
            _mediator = mediator;
            _manageCacheData = manageCacheData;
            new ObservableCollection<SubUserResponse>();
        }

        public async Task LoadHistoryAsync()
        {
            IsBusy = true;

            try
            {
                var cached = await _manageCacheData.GetUsersAsync();
                User = cached.FirstOrDefault(x => x.Id == _userIdQueryAttribute);

                var command = new GetConversationsBySubUserIdQuery()
                {
                    SubUserId = _userIdQueryAttribute
                };
                var conversationsList = await _mediator.Send(command);

                Conversations = conversationsList.ToObservableCollection();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося завантажити дані: {ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }

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
