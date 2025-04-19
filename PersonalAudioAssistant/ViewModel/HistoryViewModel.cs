using CommunityToolkit.Mvvm.ComponentModel;
using IntelliJ.Lang.Annotations;
using MediatR;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;

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
        private bool isBusy;

        public HistoryViewModel(IMediator mediator, ManageCacheData manageCacheData)
        {
            _mediator = mediator;
            _manageCacheData = manageCacheData;
        }

        public async Task LoadUserAsync()
        {
            IsBusy = true;

            try
            {
                var cached = await _manageCacheData.GetUsersAsync();
                User = cached.FirstOrDefault(x => x.Id == _userIdQueryAttribute);
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

                await LoadUserAsync();
            }
        }
    }
}
