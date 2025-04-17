

using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalAudioAssistant.Model
{
    public partial class IsNotValidAddUser : ObservableObject
    {
        [ObservableProperty]
        public bool isUserNameNotValid = false;

        [ObservableProperty]
        public bool isStartPhraseNotValid = false;

        [ObservableProperty]
        public bool isEndPhraseNotValid = false;

        [ObservableProperty]
        public bool isPasswordNotValid = false;

        [ObservableProperty]
        public bool isAudioTimeNotValid = false;        
        
        [ObservableProperty]
        public bool isCloneVoiceNotValid = false;
                
        [ObservableProperty]
        public bool isUserVoiceNotValid = false;
    }
}
