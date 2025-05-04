using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class AuthorizationViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;

        public AuthorizationViewModel(IMediator mediator, GoogleUserService googleUserService, AuthTokenManager authTokenManager)
        {
            _mediator = mediator;
            _authTokenManager = authTokenManager;
        }

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        [RelayCommand]
        private async Task SignInAsync()
        {
            IsBusy = true;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Помилка", "Будь ласка, введіть email та пароль.", "ОК");
                return;
            }

            try
            {
                await _authTokenManager.SignInWithPasswordAsync(Email, Password);
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося увійти: {ex.Message}", "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SignInGoogleAsync()
        {
            try
            {
                IsBusy = true;
                await _authTokenManager.Sign_In_AsyncGoogle();
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося увійти через Google: {ex.Message}", "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            await Shell.Current.GoToAsync("/RegistrationPage");

        }

        public async Task InitializeAsync()
        {
            try
            {
                await _authTokenManager.InitializeAsync();
            }
            catch (Exception)
            {
                await App.Current.MainPage.DisplayAlert("Помилка", "Спробуйте пізніше", "ОК");
            }
        }
    }
}
