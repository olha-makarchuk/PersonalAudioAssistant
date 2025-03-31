using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.ViewModel;

public partial class AuthorizationPage : ContentPage
{
    private readonly GoogleDriveService _googleDriveService = new();

    public AuthorizationPage()
    {
        InitializeComponent();
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await _googleDriveService.Init();
        await UpdateButtonAsync();
    }

    private async void SignInGoogle_Clicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        SignInGoogleButton.IsEnabled = false;

        if (SignInGoogleButton.Text == "Sign In")
        {
            await _googleDriveService.SignIn();
        }
        else
        {
            await _googleDriveService.SignOut();
        }

        await UpdateButtonAsync();

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        SignInGoogleButton.IsEnabled = true;
    }

    private async void SignIn_Clicked(object sender, EventArgs e)
    {

        // TODO: Додати логіку реєстрації
    }

    private async void SignUp_Clicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        // TODO: Додати логіку реєстрації
    }

    private async Task UpdateButtonAsync()
    {
        if (_googleDriveService.IsSignedIn)
        {
            SignInGoogleButton.Text = $"Sign Out ({_googleDriveService.Email})";
            NavigateToMain();
        }
        else
        {
            SignInGoogleButton.Text = "Sign In";
        }
    }

    private void NavigateToMain()
    {
        Microsoft.Maui.Controls.Application.Current.MainPage = new MainPage();
    }
}
