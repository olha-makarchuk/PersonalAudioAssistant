using PersonalAudioAssistant.ViewModel.History;

namespace PersonalAudioAssistant.Views.History;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
    }
}