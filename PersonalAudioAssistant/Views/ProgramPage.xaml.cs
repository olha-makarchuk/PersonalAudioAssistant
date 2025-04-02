using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.View;

public partial class ProgramPage : ContentPage
{

    private readonly GoogleDriveService _authTokenManager;
    public ProgramPage()
    {
        InitializeComponent();
        _authTokenManager = new GoogleDriveService();
    }

    private async void SignOut_Clicked(object sender, EventArgs e)
    {

        try
        {
            await _authTokenManager.SignOut();

        }
        catch (Exception ex)
        {
            // Якщо помилка пов'язана з неможливістю відкликання, можна її залогувати або проігнорувати
            Console.WriteLine($"Не вдалося відкликати токен: {ex.Message}");
        }


        await Shell.Current.GoToAsync("//AuthorizationPage");
    }


}