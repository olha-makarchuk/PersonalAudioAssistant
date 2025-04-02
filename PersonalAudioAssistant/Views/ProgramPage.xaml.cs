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
            // ���� ������� ���'����� � ����������� ����������, ����� �� ���������� ��� ������������
            Console.WriteLine($"�� ������� ��������� �����: {ex.Message}");
        }


        await Shell.Current.GoToAsync("//AuthorizationPage");
    }


}