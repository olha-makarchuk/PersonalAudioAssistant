using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.ViewModel;
using System.ComponentModel;

namespace PersonalAudioAssistant.Views;

public partial class ProgramPage : ContentPage
{
    private bool _hasScrolledToBottomOnce = false;

    public ProgramPage(ProgramPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        mediaElement.MediaEnded += OnMediaEnded;

        if (viewModel is INotifyPropertyChanged vmNotify)
        {
            vmNotify.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(ProgramPageViewModel.InitialLoadDone)
                    && viewModel.InitialLoadDone
                    && !_hasScrolledToBottomOnce
                    && viewModel.ChatMessages.Any())
                {
                    await Task.Delay(50);
                    ChatCollection.ScrollTo(
                        viewModel.ChatMessages.Last(),
                        position: ScrollToPosition.End,
                        animate: false);
                    _hasScrolledToBottomOnce = true;
                }
            };
        }

        viewModel.RequestScrollToEnd += () =>
        {
            if (ChatCollection.ItemsSource is IList<ChatMessage> list && list.Any())
                ChatCollection.ScrollTo(list.Last(), position: ScrollToPosition.End, animate: false);
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

        if (BindingContext is ProgramPageViewModel vm && !vm.InitialLoadDone)
        {
            await vm.LoadChatMessagesAsync();

            if (ChatCollection.ItemsSource is IList<ChatMessage> list && list.Any())
                ChatCollection.ScrollTo(list.Last(), position: ScrollToPosition.End, animate: false);
        }
    }

    private async void OnChatScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (e.FirstVisibleItemIndex == 0
            && BindingContext is ProgramPageViewModel vm
            && !vm.IsLoadingMessages
            && !vm.AllMessagesLoaded)
        {
            await vm.LoadNextPageAsync();
        }
    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is ProgramPageViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
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