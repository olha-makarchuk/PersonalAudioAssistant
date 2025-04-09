using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views
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

            await _authTokenManager.InitializeAsync();
        }
    }
}