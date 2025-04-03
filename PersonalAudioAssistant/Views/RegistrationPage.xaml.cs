using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.View
{
    public partial class RegistrationPage : ContentPage
    {
        private RegistrationPageViewModel ViewModel => BindingContext as RegistrationPageViewModel;

        public RegistrationPage(RegistrationPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        }
    }
}
