using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.View
{
    public partial class AuthorizationPage : ContentPage
    {
        AuthTokenManager _authTokenManager;
        
        public AuthorizationPage(AuthorizationViewModel viewModel, AuthTokenManager authTokenManager)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _authTokenManager = authTokenManager;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            await _authTokenManager.InitializeAsync();

            if (await _authTokenManager.IsSignedInAsync())
            {
                await _authTokenManager.SignOutAsync();
            }
        }
    }
}