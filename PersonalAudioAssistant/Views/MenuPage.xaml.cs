using CommunityToolkit.Maui.Views;

namespace PersonalAudioAssistant.Views;

public partial class MenuPage : Popup
{
    public MenuPage()
    {
        InitializeComponent();
    }

    private async void MainPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProgramPage");
        Close();
    }

    private async void UsersListPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//UsersListPage");
        Close();
    }

    private async void SettingsPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SettingsPage");
        Close();
    }

    private async void AnaliticsPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AnaliticsPage");
        Close();
    }
}
