using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
    }
}