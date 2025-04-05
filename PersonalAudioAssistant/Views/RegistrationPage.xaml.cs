using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant.Views
{
    public partial class RegistrationPage : ContentPage
    {
        private RegistrationPageViewModel ViewModel;

        public RegistrationPage(RegistrationPageViewModel viewModel)
        {
            InitializeComponent();
            Shell.SetTitleView(this, null);
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        }
    }
}
