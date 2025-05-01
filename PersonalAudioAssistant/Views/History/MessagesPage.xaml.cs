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

        mediaElement.MediaEnded += OnMediaEnded;
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

    private async void OnMediaEnded(object sender, EventArgs e)
    {
        if (BindingContext is MessagesViewModel vm && vm.IsAutoPlay)
        {
            if (vm.NextCommand.CanExecute(null))
                await vm.NextAsync();
        }
    }

    public MediaElement MediaElement => mediaElement;
}