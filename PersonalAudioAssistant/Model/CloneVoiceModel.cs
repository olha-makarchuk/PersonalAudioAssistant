using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.Model
{
    public partial class CloneVoiceModel : ObservableObject
    {
        [ObservableProperty]
        private bool isCloneVoiceSelected;

        [ObservableProperty]
        private bool isUploadSelected = true;

        [ObservableProperty]
        private bool isRecordSelected = false;

        [ObservableProperty]
        private bool isCloneAudioRecorded = false;

        [ObservableProperty]
        private bool isFragmentSelectionVisible;

        public ObservableCollection<int> HourOptions { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> MinuteOptions { get; } = new ObservableCollection<int>();
        public ObservableCollection<int> SecondOptions { get; } = new ObservableCollection<int>();

        [ObservableProperty]
        private int selectedEndHour;

        [ObservableProperty]
        private int selectedEndMinute;

        [ObservableProperty]
        private int selectedEndSecond;

        [ObservableProperty]
        private int selectedStartHour;

        [ObservableProperty]
        private int selectedStartMinute;

        [ObservableProperty]
        private int selectedStartSecond;

        [ObservableProperty]
        private TimeSpan totalDuration;
    }
}
