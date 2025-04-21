using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((SettingsViewModel)BindingContext).LoadSettingsAsync();
    }
}