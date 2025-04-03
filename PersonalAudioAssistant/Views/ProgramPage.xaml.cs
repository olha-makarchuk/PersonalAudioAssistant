using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.View;

public partial class ProgramPage : ContentPage
{

    private readonly AuthTokenManager _authTokenManager;
    public ProgramPage(AuthTokenManager authTokenManager)
    {
        InitializeComponent();
        _authTokenManager = authTokenManager;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
    }

    private async void SignOut_Clicked(object sender, EventArgs e)
    {

        try
        {
            await _authTokenManager.SignOutAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не вдалося відкликати токен: {ex.Message}");
        }

        Shell.Current?.GoToAsync("AuthorizationPage");
    }
}