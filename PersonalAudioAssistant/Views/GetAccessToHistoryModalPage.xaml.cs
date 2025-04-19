using PersonalAudioAssistant.ViewModel;
using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views;

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
}