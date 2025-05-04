using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.ViewModel;
using PersonalAudioAssistant.ViewModel.History;

namespace PersonalAudioAssistant.Views.History;
public partial class MessagesPage : ContentPage
{
    private bool _initialScrollDone = false;

    public MessagesPage(MessagesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        mediaElement.MediaEnded += OnMediaEnded;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MessagesViewModel.ChatMessages) && (BindingContext as MessagesViewModel)?.InitialLoadDone == true && !_initialScrollDone)
        {
            _initialScrollDone = true;
            await Task.Delay(200); // Даємо трохи часу на рендеринг
            try
            {
                ChatCollection?.ScrollTo((BindingContext as MessagesViewModel)?.ChatMessages.LastOrDefault(), animate: false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка початкової прокрутки (PropertyChanged): {ex.Message}");
            }
        }
        else if (e.PropertyName == nameof(MessagesViewModel.ChatMessages) && (BindingContext as MessagesViewModel)?.InitialLoadDone == true && _initialScrollDone)
        {
            try
            {
                ChatCollection?.ScrollTo((BindingContext as MessagesViewModel)?.ChatMessages.LastOrDefault(), animate: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка прокрутки (оновлення): {ex.Message}");
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MessagesViewModel vm && !vm.InitialLoadDone)
        {
            await vm.LoadChatMessagesAsync();
            vm.InitialLoadDone = true; // Встановлюємо прапорець після першого завантаження
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        if (BindingContext is MessagesViewModel viewModel)
        {
            viewModel.OnNavigatedFrom();
        }
    }

    private async void OnChatScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (e.FirstVisibleItemIndex == 0
            && BindingContext is MessagesViewModel vm
            && !vm.IsLoadingMessages
            && !vm.AllMessagesLoaded)
        {
            await vm.LoadNextPageAsync();
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