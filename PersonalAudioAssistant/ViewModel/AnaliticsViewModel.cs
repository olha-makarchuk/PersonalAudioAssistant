using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class AnaliticsViewModel : ObservableObject
    {
        [ObservableProperty]
        private View currentAnalyticsContent;

        [ObservableProperty]
        private string selectedTab = "User";

        public bool IsUserTabSelected => SelectedTab == "User";
        public bool IsTokenTabSelected => SelectedTab == "Token";
        public bool IsPaymentTabSelected => SelectedTab == "Payment";

        public AnaliticsViewModel()
        {
            CurrentAnalyticsContent = new Label { Text = "Вміст: За користувачами" };
        }

        [RelayCommand]
        private void SelectUserTab()
        {
            SelectedTab = "User";
            CurrentAnalyticsContent = new Label { Text = "Вміст: За користувачами" };
            OnPropertyChanged(nameof(IsUserTabSelected));
            OnPropertyChanged(nameof(IsTokenTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));
        }

        [RelayCommand]
        private void SelectTokenTab()
        {
            SelectedTab = "Token";
            CurrentAnalyticsContent = new Label { Text = "Вміст: Використання токенів" };
            OnPropertyChanged(nameof(IsUserTabSelected));
            OnPropertyChanged(nameof(IsTokenTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));
        }

        [RelayCommand]
        private void SelectPaymentTab()
        {
            SelectedTab = "Payment";
            CurrentAnalyticsContent = new Label { Text = "Вміст: Історія оплати" };
            OnPropertyChanged(nameof(IsUserTabSelected));
            OnPropertyChanged(nameof(IsTokenTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isActive = (bool)value;
            string param = parameter?.ToString();

            return (param == "Active" && isActive) || (param == "Inactive" && !isActive)
                ? Colors.DarkBlue
                : Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
