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



/*
using MediatR;
using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.View;

public partial class AuthorizationPage : ContentPage
{
    private readonly IMediator _mediator;
    private readonly AuthTokenManager _authTokenManager;
    public AuthorizationPage(IMediator mediator, GoogleUserService googleUserService)
    {
        InitializeComponent();
        
        _mediator = mediator;
        _authTokenManager = new AuthTokenManager(googleUserService, _mediator);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _authTokenManager.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", "Спробуйте пізніше", "ОК");
        }
    }

    private async void SignInGoogle_Clicked(object sender, EventArgs e)
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            SignInGoogleButton.IsEnabled = false;
            await _authTokenManager.Sign_In_Up_AsyncGoogle();

            await Shell.Current.GoToAsync("//ProgramPage"); 
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося увійти через Google: {ex.Message}", "ОК");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            SignInGoogleButton.IsEnabled = true;
        }
    }

    private async void SignIn_Clicked(object sender, EventArgs e)
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            SignInButton.IsEnabled = false;

            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Помилка", "Будь ласка, введіть email та пароль.", "ОК");
                return;
            }

            await _authTokenManager.SignInWithPasswordAsync(email, password);
           
            await Shell.Current.GoToAsync("//ProgramPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося увійти: {ex.Message}", "ОК");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            SignInButton.IsEnabled = true;
        }
    }

    private async void SignUp_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RegistrationPage");
    }
}
*/