using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.Model
{
    public partial class VoiceFilterModel : ObservableObject
    {
        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string age;

        [ObservableProperty]
        private string gender;

        [ObservableProperty]
        private string useCase;

        [ObservableProperty]
        private ObservableCollection<string> accentOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> descriptionOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> ageOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> genderOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> useCaseOptions = new ObservableCollection<string>();
    }
}
