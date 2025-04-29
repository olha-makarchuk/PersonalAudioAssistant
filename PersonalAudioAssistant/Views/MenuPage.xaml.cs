using CommunityToolkit.Maui.Views;
using Mopups.Interfaces;
using Mopups.Services;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views.History;
using System.ComponentModel;

namespace PersonalAudioAssistant.Views;

public partial class MenuPage : Popup, INotifyPropertyChanged
{
    private readonly IPopupNavigation _popupNavigation;

    public MenuPage() : this(MopupService.Instance) { }

    public MenuPage(IPopupNavigation popupNavigation)
    {
        InitializeComponent();
        _popupNavigation = popupNavigation;
        BindingContext = this;

        GetBalance();
    }

    private decimal _balance;
    public decimal Balance
    {
        get => _balance;
        set
        {
            if (_balance != value)
            {
                _balance = value;
                OnPropertyChanged(nameof(Balance));
            }
        }
    }

    private async void GetBalance()
    {
        var balance = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetRequiredService<ManageCacheData>();
        var settings = await balance.GetAppSettingsAsync();
        Balance = settings.balance;
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
