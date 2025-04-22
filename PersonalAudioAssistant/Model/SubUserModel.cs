using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalAudioAssistant.Model
{
    public partial class SubUserModel : ObservableObject
    {

        [ObservableProperty]
        public string startPhrase;

        [ObservableProperty]
        public string endPhrase;

        [ObservableProperty]
        public string endTime;

        [ObservableProperty]
        public string voiceId;

        [ObservableProperty]
        public List<double> userVoice;

        [ObservableProperty]
        public string userName;

        [ObservableProperty]
        public string password;

        [ObservableProperty]
        public string photoPath;

        [ObservableProperty]
        public bool isPasswordEnabled = false;
    }
}
