using PersonalAudioAssistant.ViewModel.History;
using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views.History;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is HistoryViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }
}