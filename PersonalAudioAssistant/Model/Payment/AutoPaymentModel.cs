using CommunityToolkit.Mvvm.ComponentModel;

namespace PersonalAudioAssistant.Model.Payment
{
    public partial class AutoPaymentModel : ObservableObject
    {
        [ObservableProperty]
        private bool isAutoPaymentEnabled;

        [ObservableProperty]
        private int minimumTokenBalance;

        [ObservableProperty]
        private int autoRechargeAmount;
    }
}
