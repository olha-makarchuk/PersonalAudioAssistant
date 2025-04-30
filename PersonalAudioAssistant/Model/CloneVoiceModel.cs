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
        private string cloneSourceButtonText = "Обрати новий голос";

        private Color _cloneSourceButtonColor = Color.FromArgb("#4CAF50");
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
                : "Обрати новий голос";

            CloneSourceButtonColor = IsCreateCloneVoiceMode
                ? Color.FromArgb("#ae2f2f")
                : Color.FromArgb("#4CAF50"); 

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

        public void ResetCloneSourceButton()
        {
            IsCreateCloneVoiceMode = false;
            CloneSourceButtonText = "Обрати новий голос";
            CloneSourceButtonColor = Color.FromArgb("#4CAF50");
            IsUploadSelected = false;
            IsRecordSelected = false;
        }

        public void Reset()
        {
            IsCloneGenerated = false;
            Name = string.Empty;
            IsCloneVoiceSelected = false;
            IsCloneAudioRecorded = false;
            IsFragmentSelectionVisible = false;
            IsUploadSelected = false;
            IsRecordSelected = false;
            IsCreateCloneVoiceMode = false;
            CloneSourceButtonText = "Обрати новий голос";
            CloneSourceButtonColor = Color.FromArgb("#4CAF50");
        }
    }
}
