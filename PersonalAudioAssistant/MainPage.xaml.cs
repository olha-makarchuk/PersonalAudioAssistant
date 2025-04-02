using MediatR;
using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant
{
    public partial class MainPage : ContentPage
    {
        private AuthTokenManager _authTokenManager;
        public MainPage(IMediator mediator, GoogleUserService googleUserService)
        {
            InitializeComponent();
            _authTokenManager = new AuthTokenManager(googleUserService, mediator);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeApp();
        }

        private async Task InitializeApp()
        {
            if (_authTokenManager == null)
            {
                Console.WriteLine("Error: _authTokenManager is null.");
                return;
            }

            await _authTokenManager.InitializeAsync();

            if (_authTokenManager.IsSignedIn)
            {
                Shell.Current?.GoToAsync("//ProgramPage");
            }
            else
            {
                Shell.Current?.GoToAsync("//AuthorizationPage");
            }
        }
    }
}
