using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.ViewModel;

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

        if (BindingContext is ProgramPageViewModel vm && !vm.InitialLoadDone)
        {
            await vm.LoadInitialChatAsync();
            // скролимо до к≥нц€ лише 1 раз
            if (ChatCollection.ItemsSource is IList<ChatMessage> list && list.Any())
                ChatCollection.ScrollTo(list.Last(), position: ScrollToPosition.End, animate: false);
        }
    }

    private async void OnChatScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        // завантажуЇмо лише коли перший елемент видимий
        if (e.FirstVisibleItemIndex == 0
            && BindingContext is ProgramPageViewModel vm
            && !vm.IsLoadingMessages
            && !vm.AllMessagesLoaded)
        {
            await vm.LoadNextPageAsync();
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