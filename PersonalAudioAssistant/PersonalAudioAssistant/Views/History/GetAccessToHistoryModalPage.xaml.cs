using PersonalAudioAssistant.ViewModel.History;

namespace PersonalAudioAssistant.Views.History;

public partial class GetAccessToHistoryModalPage
{
    public GetAccessToHistoryModalPage(GetAccessToHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GetAccessToHistoryViewModel viewModel)
        {
            await viewModel.LoadUsersAsync();
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is GetAccessToHistoryViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }
}