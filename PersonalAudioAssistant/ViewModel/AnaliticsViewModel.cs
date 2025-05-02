using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Contracts.PaymentHistory;
using PersonalAudioAssistant.Services.Api;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using Microcharts;


namespace PersonalAudioAssistant.ViewModel
{
    public partial class AnaliticsViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly MoneyUsedApiClient _moneyUsedApiClient;
        private readonly MoneyUsersUsedApiClient _moneyUsersUsedApiClient;

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
        public AnaliticsViewModel(IMediator mediator, PaymentHistoryApiClient paymentHistoryApiClient, MoneyUsedApiClient moneyUsedApiClient, MoneyUsersUsedApiClient moneyUsersUsedApiClient)
        {
            CurrentAnalyticsContent = new Label { Text = "Вміст: За користувачами" };
            _mediator = mediator;
            _paymentHistoryApiClient = paymentHistoryApiClient;
            HistoryList = new ObservableCollection<PaymentHistoryResponse>();
            _moneyUsedApiClient = moneyUsedApiClient;
            _moneyUsersUsedApiClient = moneyUsersUsedApiClient;
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

        [ObservableProperty]
        private Chart userMoneyPieChart;

        public async Task LoadUserMoneyUsedAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var list = await _moneyUsersUsedApiClient.GetMoneyUsersUsedAsync(userId);

            if (list == null || !list.Any())
            {
                await Shell.Current.DisplayAlert("Попередження", "Немає даних для відображення.", "OK");
                return;
            }

            // Формуємо дані для кругової діаграми
            var entries = list.Select(user => new ChartEntry((float)user.amountMoney)
            {
                Label = user.subUserId,
                ValueLabel = user.amountMoney.ToString("F2"),
                Color = SKColor.Parse("#" + RandomColor())
            }).ToList();

            UserMoneyPieChart = new PieChart
            {
                Entries = entries,
                LabelTextSize = 30,
                BackgroundColor = SKColors.Transparent
            };
        }

        // Генератор випадкових кольорів
        private string RandomColor()
        {
            var random = new Random();
            return String.Format("{0:X6}", random.Next(0x1000000));
        }


        [RelayCommand]
        private async Task SelectUserTab()
        {
            SelectedTab = "User";
            CurrentAnalyticsContent = new Label { Text = "Вміст: За користувачами" };
            OnPropertyChanged(nameof(IsUserTabSelected));
            OnPropertyChanged(nameof(IsTokenTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await LoadUserMoneyUsedAsync();
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
