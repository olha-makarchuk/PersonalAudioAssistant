using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class CloneVoicePage : ContentPage
{
	public CloneVoicePage(CloneVoiceViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}