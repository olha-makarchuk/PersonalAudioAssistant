using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class ProgramPage : ContentPage
{

    public ProgramPage(ProgramPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
    }
}