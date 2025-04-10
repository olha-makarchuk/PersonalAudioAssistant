using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;
        private readonly ManageCacheData _manageCacheData;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private int balance;

        [ObservableProperty]
        private string theme;

        [ObservableProperty]
        private ObservableCollection<string> themes = new ObservableCollection<string>
        {
            "Light",
            "Dark"
        };

        public SettingsViewModel(
            IMediator mediator,
            AuthTokenManager authTokenManager,
            ManageCacheData manageCacheData)
        {
            _mediator = mediator;
            _authTokenManager = authTokenManager;
            _manageCacheData = manageCacheData;

            LoadSettingsAsync();
        }

        private async void LoadSettingsAsync()
        {
            try
            {
                var settings = await _manageCacheData.GetAppSettingsAsync();
                Email = await SecureStorage.GetAsync("user_email");
                Balance = settings.Balance;
                Theme = settings.Theme;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task SaveSettingsAsync()
        {
        }

        [RelayCommand]
        public async Task PaymentDetails()
            => await Shell.Current.GoToAsync("/PaymentPage");

        [RelayCommand]
        public async Task SignOut()
        {
            await _authTokenManager.SignOutAsync();
            await Shell.Current.GoToAsync("//AuthorizationPage");
        }
    }
}
