using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using System.Text.RegularExpressions;

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

        [ObservableProperty]
        private bool isEmailNotValid;

        [ObservableProperty]
        private bool isPasswordNotValid;

        [ObservableProperty]
        private string emailValidationMessage;

        [ObservableProperty]
        private string passwordValidationMessage;

        partial void OnEmailChanged(string value)
        {
            ValidateEmail(value);
        }

        partial void OnPasswordChanged(string value)
        {
            ValidatePassword(value);
        }

        private void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                IsEmailNotValid = true;
                EmailValidationMessage = "Електронна пошта обов'язкова.";
                return;
            }

            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                IsEmailNotValid = true;
                EmailValidationMessage = "Некоректний формат електронної пошти.";
                return;
            }

            IsEmailNotValid = false;
            EmailValidationMessage = string.Empty;
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                IsPasswordNotValid = true;
                PasswordValidationMessage = "Пароль обов'язковий.";
                return;
            }

            IsPasswordNotValid = false;
            PasswordValidationMessage = string.Empty;
            // За потреби можна додати складніші перевірки пароля тут
        }

        [RelayCommand]
        private async Task SignInAsync()
        {
            ValidateEmail(Email);
            ValidatePassword(Password);

            if (IsEmailNotValid || IsPasswordNotValid)
            {
                return;
            }

            IsBusy = true;

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