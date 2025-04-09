using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}