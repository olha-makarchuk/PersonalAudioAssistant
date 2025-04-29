using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;
        private readonly ManageCacheData _manageCacheData;
        private readonly MainUserApiClient _mainUserApiClient;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private decimal balance;

        [ObservableProperty]
        private string theme;

        [ObservableProperty]
        private ObservableCollection<string> themes = new ObservableCollection<string>
        {
            "Light",
            "Dark"
        };

        [ObservableProperty]
        private string oldPassword;

        [ObservableProperty]
        private string newPassword;

        public SettingsViewModel(
            IMediator mediator,
            AuthTokenManager authTokenManager,
            ManageCacheData manageCacheData,
            MainUserApiClient mainUserApiClient)
        {
            _mediator = mediator;
            _authTokenManager = authTokenManager;
            _manageCacheData = manageCacheData;
            _mainUserApiClient = mainUserApiClient;
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                var settings = await _manageCacheData.GetAppSettingsAsync();
                Email = await SecureStorage.GetAsync("user_email");
                Balance = settings.balance;
                Theme = settings.theme;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task ChangePassword()
        {
            try
            {
                var email = await SecureStorage.GetAsync("user_email");
                if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
                {
                    await Shell.Current.DisplayAlert("Помилка", "Будь ласка, заповніть обидва поля пароля.", "ОК");
                    return;
                }

                await _mainUserApiClient.ChangePasswordAsync(email, OldPassword, NewPassword);

                OldPassword = string.Empty;
                NewPassword = string.Empty;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка при зміні пароля: {ex.Message}", "ОК");
            }
        }

        [RelayCommand]
        public async Task PaymentDetails()
            => await Shell.Current.GoToAsync("/PaymentPage");

        [RelayCommand]
        public async Task SignOut()
        {
            var answer = await Shell.Current.DisplayAlert(
                "Вихід",
                "Ви дійсно хочете вийти з вашого акаунту?",
                "Так",
                "Ні");

            if (answer)
            {
                await _authTokenManager.SignOutAsync();
                _manageCacheData.ClearCache();
                await Shell.Current.GoToAsync("//AuthorizationPage");
            }
        }

        [RelayCommand] 
        public async Task SaveTheme()
        {
            
        }
    }
}
