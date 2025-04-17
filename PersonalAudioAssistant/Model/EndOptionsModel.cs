using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.Model
{
    public partial class EndOptionsModel : ObservableObject
    {
        [ObservableProperty]
        private bool isEndPhraseSelected = false;

        [ObservableProperty]
        private bool isEndTimeSelected = true;

        [ObservableProperty]
        private int selectedEndTime = 2;

        [ObservableProperty]
        private ObservableCollection<int> endTimeOptions = new ObservableCollection<int>(Enumerable.Range(2, 9));
    }
}
