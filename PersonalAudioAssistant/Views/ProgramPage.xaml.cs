using AndroidX.Lifecycle;
using CommunityToolkit.Maui.Views;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views;

public partial class ProgramPage : ContentPage
{

    private readonly AuthTokenManager _authTokenManager;
    public ProgramPage(AuthTokenManager authTokenManager, ProgramPageViewModel viewModel)
    {
        InitializeComponent();
        _authTokenManager = authTokenManager;
        //BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
    }
}