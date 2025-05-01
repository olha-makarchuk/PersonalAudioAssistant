using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.ViewModel;
using PersonalAudioAssistant.ViewModel.History;

namespace PersonalAudioAssistant.Views;

public partial class ProgramPage : ContentPage
{

    public ProgramPage(ProgramPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        mediaElement.MediaEnded += OnMediaEnded;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

        if (BindingContext is ProgramPageViewModel viewModel)
        {
            await viewModel.LoadInitialChatAsync();
        }
    }

    private async void OnChatScrolled(object sender, ScrolledEventArgs e)
    {
        if (BindingContext is ProgramPageViewModel viewModel)
        {
            await viewModel.LoadMoreMessagesIfNeeded(e.ScrollY);
        }
    }

    private async void OnMediaEnded(object sender, EventArgs e)
    {
        if (BindingContext is ProgramPageViewModel vm && vm.IsAutoPlay)
        {
            if (vm.NextCommand.CanExecute(null))
                await vm.NextAsync();
        }
    }

    public MediaElement MediaElement => mediaElement;
}