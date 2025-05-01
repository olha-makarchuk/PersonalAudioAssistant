using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class RegistrationPageViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;
        private PaymentApiClient _paymentApiClient;
        private AutoPaymentApiClient _autoPaymentApiClient;
        private ConversationApiClient _conversationApiClient;

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
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        private AppSettingsApiClient _appSettingsApiClient;

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

            await _paymentApiClient.CreatePaymentAsync(userId);

            await _autoPaymentApiClient.CreateAutoPaymentAsync(userId);

            await _appSettingsApiClient.CreateAppSettingsAsync(userId);

            await _conversationApiClient.CreateConversationAsync("", userId);

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
