using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalAudioAssistant.Model.Payment
{
    public partial class CardModel : ObservableObject
    {
        [ObservableProperty]
        private string maskedCardNumber;

        [ObservableProperty]
        public string cardNumber;

        [ObservableProperty]
        private string cVV_number;

        [ObservableProperty]
        private string dateExpirience;
    }
}
