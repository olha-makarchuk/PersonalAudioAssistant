using PersonalAudioAssistant.ViewModel.History;

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
