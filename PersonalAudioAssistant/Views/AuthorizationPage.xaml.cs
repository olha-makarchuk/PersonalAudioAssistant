using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.View
{
    public partial class AuthorizationPage : ContentPage
    {
        private AuthorizationViewModel ViewModel => BindingContext as AuthorizationViewModel;

        public AuthorizationPage(AuthorizationViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
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
            await DisplayAlert("�������", "��������� �����", "��");
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
            await DisplayAlert("�������", $"�� ������� ����� ����� Google: {ex.Message}", "��");
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
                await DisplayAlert("�������", "���� �����, ������ email �� ������.", "��");
                return;
            }

            await _authTokenManager.SignInWithPasswordAsync(email, password);
           
            await Shell.Current.GoToAsync("//ProgramPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("�������", $"�� ������� �����: {ex.Message}", "��");
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