using AndroidX.Lifecycle;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.View;

public partial class ProgramPage : ContentPage
{

    private readonly AuthTokenManager _authTokenManager;
    public ProgramPage(AuthTokenManager authTokenManager, ProgramPageViewModel viewModel)
    {
        InitializeComponent();
        _authTokenManager = authTokenManager;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
    }
}