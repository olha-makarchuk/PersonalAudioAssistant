using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant
{
    public partial class AppShell : Shell
    {
        private readonly GoogleDriveService _googleDriveService;

        public AppShell()
        {
            InitializeComponent();
            _googleDriveService = new GoogleDriveService();
            InitializeApp();
        }

        private async void InitializeApp()
        {
            await _googleDriveService.Init();
            if (_googleDriveService.IsSignedIn)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.GoToAsync("//AuthorizationPage");
            }
        }
    }
}