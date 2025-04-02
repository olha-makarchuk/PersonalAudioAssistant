using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using System;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class AuthorizationViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;

        public AuthorizationViewModel(IMediator mediator, GoogleUserService googleUserService)
        {
            _mediator = mediator;
            _authTokenManager = new AuthTokenManager(googleUserService, _mediator);
        }

        // Властивості для зв’язування з Entry
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        // Прапорець для індикації завантаження
        [ObservableProperty]
        private bool isBusy;

        // Команда для входу за допомогою email/паролю
        [RelayCommand]
        private async Task SignInAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Помилка", "Будь ласка, введіть email та пароль.", "ОК");
                return;
            }

            try
            {
                IsBusy = true;
                await _authTokenManager.SignInWithPasswordAsync(Email, Password);
                await Shell.Current.GoToAsync("//ProgramPage");
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

        // Команда для входу через Google
        [RelayCommand]
        private async Task SignInGoogleAsync()
        {
            try
            {
                IsBusy = true;
                await _authTokenManager.Sign_In_Up_AsyncGoogle();
                await Shell.Current.GoToAsync("//ProgramPage");
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

        // Команда для переходу до сторінки реєстрації
        [RelayCommand]
        private async Task SignUpAsync()
        {
            await Shell.Current.GoToAsync("//RegistrationPage");
        }

        // Метод для ініціалізації, який викликається при появі сторінки
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
