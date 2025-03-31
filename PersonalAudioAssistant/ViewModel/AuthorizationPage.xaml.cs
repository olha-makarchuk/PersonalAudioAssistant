using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.ViewModel;

public partial class AuthorizationPage : ContentPage
{
	public AuthorizationPage()
	{
		InitializeComponent();
	}

    readonly GoogleDriveService _googleDriveService = new();

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await _googleDriveService.Init();
        UpdateButtonAsync();
    }

    private async void SignIn_Clicked(object sender, EventArgs e)
    {
        if (SignInButton.Text == "Sign In")
        {
            await _googleDriveService.SignIn();
        }
        else
        {
            await _googleDriveService.SignOut();

        }
        UpdateButtonAsync();
    }

    private async void NavigateToMain()
    {
        Microsoft.Maui.Controls.Application.Current.MainPage = new MainPage();
    }

    private async Task UpdateButtonAsync()
    {
        if (_googleDriveService.IsSignedIn)
        {
            SignInButton.Text = $"Sign Out ({_googleDriveService.Email})";
            NavigateToMain();
        }
        else
        {
            SignInButton.Text = "Sign In";
        }
    }
}