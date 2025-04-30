using CommunityToolkit.Mvvm.ComponentModel;
using Google.Apis.Drive.v3.Data;

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

        public void Reset()
        {
            StartPhrase = string.Empty;
            EndPhrase = string.Empty;
            EndTime = string.Empty;
            VoiceId = string.Empty;
            UserVoice = new List<double>();
            UserName = string.Empty;
            Password = string.Empty;
            IsPasswordEnabled = false;
            PhotoPath = string.Empty;
            IsPasswordEnabled = false;
        }
    }
}
