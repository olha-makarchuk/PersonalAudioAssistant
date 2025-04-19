using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class GetAccessToHistoryModalPage
{
    public GetAccessToHistoryModalPage(GetAccessToHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        this.Appearing += async (_, __) => await viewModel.RefreshUsersAsync();
    }
}