using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalAudioAssistant.Model
{
    public partial class CredentialsModelUpdate : ObservableObject
    {
        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string oldPassword;

        [ObservableProperty]
        private string newPassword;

        [ObservableProperty]
        private bool isPasswordEnabled = false;
    }
}
