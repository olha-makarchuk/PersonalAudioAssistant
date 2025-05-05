using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class RegistrationPageViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;
        private PaymentApiClient _paymentApiClient;
        private AutoPaymentApiClient _autoPaymentApiClient;
        private ConversationApiClient _conversationApiClient;
        private TokenResponse _tokenResponse;
        private AppSettingsApiClient _appSettingsApiClient;

        public RegistrationPageViewModel(IMediator mediator, GoogleUserService googleUserService, AppSettingsApiClient appSettingsApiClient, PaymentApiClient paymentApiClient, AutoPaymentApiClient autoPaymentApiClient, AuthTokenManager authTokenManager, ConversationApiClient conversationApiClient)
        {
            _mediator = mediator;
            _authTokenManager = authTokenManager;
            _appSettingsApiClient = appSettingsApiClient;
            _paymentApiClient = paymentApiClient;
            _autoPaymentApiClient = autoPaymentApiClient;
            _conversationApiClient = conversationApiClient;
        }

        [ObservableProperty]
        private bool ifCompleteRegistrationVisible;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string repeatPassword;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isPasswordMismatch;

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
            ValidatePasswordsMatch();
        }

        partial void OnRepeatPasswordChanged(string value)
        {
            ValidatePasswordsMatch();
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

            if (password.Length < 6)
            {
                IsPasswordNotValid = true;
                PasswordValidationMessage = "Пароль повинен містити щонайменше 6 символів.";
                return;
            }

            if (!Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d).+$"))
            {
                IsPasswordNotValid = true;
                PasswordValidationMessage = "Пароль повинен містити літери та цифри.";
                return;
            }

            IsPasswordNotValid = false;
            PasswordValidationMessage = string.Empty;
        }

        private void ValidatePasswordsMatch()
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(RepeatPassword))
            {
                IsPasswordMismatch = Password != RepeatPassword;
            }
            else
            {
                IsPasswordMismatch = false;
            }
        }

        [RelayCommand]
        private async Task SignUpAsync()
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
                await _authTokenManager.SignUpWithPasswordAsync(Email, Password);
                await CreateDbData();
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося зареєструватися: {ex.Message}", "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SignUpGoogleAsync()
        {
            try
            {
                IsBusy = true;
                _tokenResponse = await _authTokenManager.Sign_Up_AsyncGoogle();
                IfCompleteRegistrationVisible = true;
                Password = null;
                // Очищаємо повідомлення про помилки, якщо вони були
                IsPasswordNotValid = false;
                PasswordValidationMessage = string.Empty;
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
        private async Task CompleteSignUpGoogleAsync()
        {
            ValidatePassword(Password);
            ValidatePasswordsMatch();

            if (IsPasswordNotValid || IsPasswordMismatch)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _authTokenManager.Complete_Sign_Up_AsyncGoogle(_tokenResponse, Password);
                await CreateDbData();
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося завершити реєстрацію: {ex.Message}", "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SignInAsync()
        {
            await Shell.Current.GoToAsync("//AuthorizationPage");
        }

        private async Task CreateDbData()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            await _paymentApiClient.CreatePaymentAsync(userId);
            await _autoPaymentApiClient.CreateAutoPaymentAsync(userId);
            await _appSettingsApiClient.CreateAppSettingsAsync(userId);
            await _conversationApiClient.CreateConversationAsync("", userId);
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