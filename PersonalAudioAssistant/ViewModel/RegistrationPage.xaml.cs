using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.ViewModel;

public partial class RegistrationPage : ContentPage
{
    private readonly GoogleDriveService _googleDriveService = new();

    public RegistrationPage()
    {
        InitializeComponent();
        Loaded += ContentPage_Loaded;
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        try
        {
            await _googleDriveService.Init();
        }
        catch (Exception ex)
        {
            await DisplayAlert("�������", $"������� ����������� Google Drive: {ex.Message}", "OK");
        }
    }

    private async void RegistrationWithGoogle_Clicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        GoogleSignInButton.IsEnabled = false;

        try
        {
            await _googleDriveService.SignIn();
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("�������", $"������� ��������� ����� Google: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            GoogleSignInButton.IsEnabled = true;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("�������", "���� �����, �������� �� ����.", "OK");
            return;
        }

        // TODO: ������ ����� ��� ��������� �����������

        await DisplayAlert("������", $"��������� ������! Email: {email}", "OK");
        await Shell.Current.GoToAsync("//MainPage");
    }
}
