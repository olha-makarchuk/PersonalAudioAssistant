using CommunityToolkit.Maui.Views;
using Mopups.Interfaces;
using Mopups.Pages;
using Mopups.Services;
using Microsoft.Maui;     
using Microsoft.Extensions.DependencyInjection;

namespace PersonalAudioAssistant.Views;

public partial class MenuPage : Popup
{
    private readonly IPopupNavigation _popupNavigation;

    public MenuPage() : this(MopupService.Instance) { }

    public MenuPage(IPopupNavigation popupNavigation)
    {
        InitializeComponent();
        _popupNavigation = popupNavigation;
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

    private async void HistoryModalPageClicked(object sender, EventArgs e)
    {
        try
        {
            Close();

            var modal = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetRequiredService<GetAccessToHistoryModalPage>();
            await _popupNavigation.PushAsync(modal);
        }
        catch (Exception ex)
        {
            await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
        }
    }

}
