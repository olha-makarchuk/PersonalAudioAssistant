using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Contracts.PaymentHistory;
using PersonalAudioAssistant.Services.Api;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class AnaliticsViewModel : ObservableObject
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private View currentAnalyticsContent;

        [ObservableProperty]
        private string selectedTab = "User";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private ObservableCollection<PaymentHistoryResponse> historyList;

        public bool IsUserTabSelected => SelectedTab == "User";
        public bool IsTokenTabSelected => SelectedTab == "Token";
        public bool IsPaymentTabSelected => SelectedTab == "Payment";

        private PaymentHistoryApiClient _paymentHistoryApiClient;
        public AnaliticsViewModel(IMediator mediator, PaymentHistoryApiClient paymentHistoryApiClient)
        {
            CurrentAnalyticsContent = new Label { Text = "Вміст: За користувачами" };
            _mediator = mediator;
            _paymentHistoryApiClient = paymentHistoryApiClient;
            HistoryList = new ObservableCollection<PaymentHistoryResponse>();
        }

        public async Task LoadHistoryPayment()
        {
            var userId = await SecureStorage.GetAsync("user_id");

            var list = await _paymentHistoryApiClient.GetPaymentHistoryByUserIdAsync(userId);

            if (list == null || !list.Any())
            {
                await Shell.Current.DisplayAlert("Попередження", "Немає даних для відображення.", "OK");
                return;
            }

            foreach (var item in list)
            {
                HistoryList.Add(item);
            }
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
        private async Task SelectPaymentTabAsync()
        {
            SelectedTab = "Payment";
            CurrentAnalyticsContent = new Label { Text = "Вміст: Історія оплати" };
            OnPropertyChanged(nameof(IsUserTabSelected));
            OnPropertyChanged(nameof(IsTokenTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                HistoryList.Clear();
                await LoadHistoryPayment();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
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
