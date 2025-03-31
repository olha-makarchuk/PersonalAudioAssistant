using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;
using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;

namespace PersonalAudioAssistant
{
    public  partial class MainPage : ContentPage
    {
        private readonly GoogleDriveService _googleDriveService = new();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void SignOut_Clicked(object sender, EventArgs e)
        {
            await _googleDriveService.SignOut();
            Microsoft.Maui.Controls.Application.Current.MainPage = new AuthorizationPage();
        }
    }
}
