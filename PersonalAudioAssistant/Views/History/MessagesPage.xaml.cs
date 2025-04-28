using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.ViewModel.History;

namespace PersonalAudioAssistant.Views.History;
public partial class MessagesPage : ContentPage
{
	public MessagesPage(MessagesViewModel viewModel)
	{
		InitializeComponent();
        Shell.SetTitleView(this, null);
        BindingContext = viewModel;
    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is MessagesViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MessagesViewModel viewModel)
        {
            await viewModel.LoadMessagesAsync();
        }
    }

    public MediaElement MediaElement => mediaElement;
}