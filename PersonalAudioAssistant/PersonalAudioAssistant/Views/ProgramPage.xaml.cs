using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.ViewModel;
using System.Collections.Specialized;
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
        ChatCollection.Scrolled += OnChatScrolled;

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

        viewModel.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;

        viewModel.RequestScrollToEnd += () =>
        {
            if (ChatCollection.ItemsSource is IList<ChatMessage> list && list.Any())
                ChatCollection.ScrollTo(list.Last(), position: ScrollToPosition.End, animate: false);
        };
    }

    private void ChatMessages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Якщо добавили хоча б один новий елемент
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            // Прокрутимо на UI-потоке
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Дрібна затримка, щоб CollectionView встиг оновитися
                await Task.Delay(50);

                // Беремо останнє повідомлення з BindingContext
                if (BindingContext is ProgramPageViewModel vm && vm.ChatMessages.Any())
                {
                    var last = vm.ChatMessages.Last();
                    ChatCollection.ScrollTo(
                        last,
                        position: ScrollToPosition.End,
                        animate: true);
                }
            });
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

        if (BindingContext is ProgramPageViewModel vm && !vm.InitialLoadDone)
        {
            //await vm.LoadChatMessagesAsync();

            if (ChatCollection.ItemsSource is IList<ChatMessage> list && list.Any())
                ChatCollection.ScrollTo(list.Last(), position: ScrollToPosition.End, animate: false);
        }
    }
    private int _firstVisibleIndex;
    private void OnChatScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        _firstVisibleIndex = e.FirstVisibleItemIndex;
        // якщо це догрузка (коли дійшли до нуля), запускаємо завантаження
        if (_firstVisibleIndex == 0
            && BindingContext is ProgramPageViewModel vm
            && !vm.IsLoadingMessages
            && !vm.AllMessagesLoaded
            && !vm.IsPrivateConversation) // Додана умова для перевірки приватної розмови
        {
            _ = LoadMoreKeepingOffsetAsync(vm);
        }
    }

    private EventHandler<ItemsViewScrolledEventArgs> _scrollHandler;
    // Завантажуємо нові сторінки і потім “перекочуємо” назад
    private async Task LoadMoreKeepingOffsetAsync(ProgramPageViewModel vm)
    {
        // Скільки було до завантаження
        var oldCount = vm.ChatMessages.Count;

        // Дочікуємося додавання нових
        await vm.LoadNextPageAsync();

        var newCount = vm.ChatMessages.Count;
        var added = newCount - oldCount;
        if (added <= 0)
            return;

        // Після того, як дані вставились, прокручуємо до того ж елемента,
        // зміщеного на кількість нових
        var targetIndex = _firstVisibleIndex + added;
        if (targetIndex < vm.ChatMessages.Count)
        {
            // Невелика затримка, щоб CollectionView встиг оновитись
            await Task.Delay(50);
            ChatCollection.ScrollTo(
                vm.ChatMessages[targetIndex],
                position: ScrollToPosition.Start,
                animate: false);
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