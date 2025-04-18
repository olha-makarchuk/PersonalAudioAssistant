using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.Model
{
    public partial class CloneVoiceModel : ObservableObject
    {
        [ObservableProperty]
        private bool isCloneGenerated = false;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string description;        
        
        [ObservableProperty]
        private bool isCloneVoiceSelected;

        [ObservableProperty]
        private bool isCloneAudioRecorded = false;

        [ObservableProperty]
        private bool isFragmentSelectionVisible;

        [ObservableProperty]
        private bool isUploadSelected = false;

        [ObservableProperty]
        private bool isRecordSelected = false;

        [ObservableProperty]
        private bool isCreateCloneVoiceMode = false;

        [ObservableProperty]
        private string cloneSourceButtonText = "Зклонувати новий голос";

        private Color _cloneSourceButtonColor;
        public Color CloneSourceButtonColor
        {
            get => _cloneSourceButtonColor;
            set => SetProperty(ref _cloneSourceButtonColor, value);
        }

        [RelayCommand]
        private void ToggleCloneSource()
        {
            IsCreateCloneVoiceMode = !IsCreateCloneVoiceMode;

            CloneSourceButtonText = IsCreateCloneVoiceMode
                ? "Скасувати"
                : "Зклонувати новий голос";

            CloneSourceButtonColor = IsCreateCloneVoiceMode
                ? Colors.Red
                : Colors.Green;

            if (IsCreateCloneVoiceMode)
            {
                IsUploadSelected = true;
                IsRecordSelected = false;
            }
            else
            {
                IsUploadSelected = false;
                IsRecordSelected = false;
            }
        }


    }
}
