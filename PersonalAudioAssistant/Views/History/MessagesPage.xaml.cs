using PersonalAudioAssistant.ViewModel.History;
using PersonalAudioAssistant.ViewModel.Users;

namespace PersonalAudioAssistant.Views.History;

public partial class MessagesPage : ContentPage
{
	public MessagesPage(MessagesViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MessagesViewModel viewModel)
        {
            await viewModel.LoadMessagesAsync();
        }
    }
}