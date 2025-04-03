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
            // Якщо помилка пов'язана з неможливістю відкликання, можна її залогувати або проігнорувати
            Console.WriteLine($"Не вдалося відкликати токен: {ex.Message}");
        }


        await Shell.Current.GoToAsync("//AuthorizationPage");
    }


}