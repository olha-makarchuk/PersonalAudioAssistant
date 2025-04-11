using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class RegistrationPageViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;

        public RegistrationPageViewModel(IMediator mediator, GoogleUserService googleUserService)
        {
            _mediator = mediator;
            _authTokenManager = new AuthTokenManager(googleUserService, _mediator);
        }

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        [RelayCommand]
        private async Task SignUpAsync()
        {
            IsBusy = true;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Помилка", "Будь ласка, введіть email та пароль.", "ОК");
                return;
            }

            try
            {
                await _authTokenManager.SignUpWithPasswordAsync(Email, Password);
                await CreateDbData();
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
        private async Task SignUnGoogleAsync()
        {
            try
            {
                IsBusy = true;
                await _authTokenManager.Sign_In_Up_AsyncGoogle();
                await CreateDbData();
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
        private async Task SignInAsync()
        {
            await Shell.Current.GoToAsync("//AuthorizationPage");
        }

        private async Task CreateDbData()
        {
            var userId = await SecureStorage.GetAsync("user_id");

            var paymentCommand = new CreatePaymentCommand() 
            { 
                UserId = userId,
            };
            await _mediator.Send(paymentCommand);

            var autoPaymentCommand = new CreateAutoPaymentCommand()
            {
                UserId = userId
            };
            await _mediator.Send(autoPaymentCommand);

            var settingsCommand = new CreateAppSettingsCommand()
            {
                UserId = userId
            };
            await _mediator.Send(settingsCommand);

            //var analiticsCommand = new CreateAnaliticsCommand() { };
            //await _mediator.Send(analiticsCommand);
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
