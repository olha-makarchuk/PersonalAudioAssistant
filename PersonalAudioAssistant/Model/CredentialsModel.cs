using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalAudioAssistant.Model
{
    public partial class CredentialsModel : ObservableObject
    {
        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isPasswordEnabled = false;
    }
}
