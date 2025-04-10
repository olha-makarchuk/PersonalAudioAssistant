using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class AnaliticsViewModel: ObservableObject
    {
        [RelayCommand]
        private async Task NavigateToHistory()
        {
            await Shell.Current.GoToAsync("/HistoryPage");

        }
    }
}
