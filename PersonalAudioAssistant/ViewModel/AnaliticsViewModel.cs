using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Contracts.PaymentHistory;
using PersonalAudioAssistant.Services.Api;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using Microcharts;
using PersonalAudioAssistant.Services;
using System.ComponentModel;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class AnaliticsViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly MoneyUsedApiClient _moneyUsedApiClient;
        private readonly MoneyUsersUsedApiClient _moneyUsersUsedApiClient;
        private readonly ManageCacheData _manageCacheData;
        private readonly PaymentHistoryApiClient _paymentHistoryApiClient;

        [ObservableProperty]
        private View currentAnalyticsContent;

        [ObservableProperty]
        private string selectedTab = "User";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private ObservableCollection<PaymentHistoryResponse> historyList;

        public bool IsMoneyUsersUsedTabSelected => SelectedTab == "User";
        public bool IsMoneyUsedTabSelected => SelectedTab == "Token";
        public bool IsPaymentTabSelected => SelectedTab == "Payment";

        [ObservableProperty]
        private Chart userMoneyPieChart;
        [ObservableProperty]
        private ObservableCollection<UserChartItem> userChartItems = new();

        [ObservableProperty]
        private LineChart monthlyUsageChart;

        [ObservableProperty]
        private string monthlyUsageChartTitle;

        [ObservableProperty]
        private bool showYearlyAnalytics = false;

        [ObservableProperty]
        private bool showMonthlyAnalytics = true; // За замовчуванням показувати за місяць

        [ObservableProperty]
        private bool showWeeklyAnalytics = false;

        public AnaliticsViewModel(IMediator mediator, PaymentHistoryApiClient paymentHistoryApiClient, MoneyUsedApiClient moneyUsedApiClient, ManageCacheData manageCasheData, MoneyUsersUsedApiClient moneyUsersUsedApiClient)
        {
            CurrentAnalyticsContent = new Label { Text = "Вміст: За користувачами" };
            _mediator = mediator;
            _paymentHistoryApiClient = paymentHistoryApiClient;
            HistoryList = new ObservableCollection<PaymentHistoryResponse>();
            _moneyUsedApiClient = moneyUsedApiClient;
            _moneyUsersUsedApiClient = moneyUsersUsedApiClient;
            _manageCacheData = manageCasheData;

            // Підписуємося на зміни властивостей чекбоксів
            PropertyChanged += AnaliticsViewModel_PropertyChanged;
            UpdateChartTitle();
            LoadUserMoneyUsedAsync();
        }

        private void UpdateChartTitle()
        {
            if (ShowYearlyAnalytics)
            {
                MonthlyUsageChartTitle = "Витрати за рік";
            }
            else if (ShowMonthlyAnalytics)
            {
                MonthlyUsageChartTitle = "Витрати за місяць";
            }
            else if (ShowWeeklyAnalytics)
            {
                MonthlyUsageChartTitle = "Витрати за тиждень";
            }
            else
            {
                MonthlyUsageChartTitle = "Витрати за місяць"; // За замовчуванням
            }
        }

        private async void AnaliticsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShowYearlyAnalytics) && ShowYearlyAnalytics)
            {
                ShowMonthlyAnalytics = false;
                ShowWeeklyAnalytics = false;
                await ReloadChart();
            }
            else if (e.PropertyName == nameof(ShowMonthlyAnalytics) && ShowMonthlyAnalytics)
            {
                ShowYearlyAnalytics = false;
                ShowWeeklyAnalytics = false;
                await ReloadChart();
            }
            else if (e.PropertyName == nameof(ShowWeeklyAnalytics) && ShowWeeklyAnalytics)
            {
                ShowYearlyAnalytics = false;
                ShowMonthlyAnalytics = false;
                await ReloadChart();
            }
        }

        private async Task ReloadChart()
        {
            UpdateChartTitle();
            await LoadMonthlyMoneyUsedAnalyticsAsync();
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

            HistoryList.Clear();
            foreach (var item in list)
            {
                HistoryList.Add(item);
            }
        }

        public async Task LoadUserMoneyUsedAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var list = await _moneyUsersUsedApiClient.GetMoneyUsersUsedAsync(userId);
            var listUsers = await _manageCacheData.GetUsersAsync();

            if (list == null || !list.Any())
            {
                await Shell.Current.DisplayAlert("Попередження", "Немає даних для відображення.", "OK");
                return;
            }

            var items = list.Select(money =>
            {
                var user = listUsers.FirstOrDefault(u => u.id == money.subUserId);
                var userName = user?.userName ?? "Невідомо";
                var avatar = user?.photoPath;
                var hex = "#" + RandomColor();
                var skColor = SKColor.Parse(hex);
                var mauiColor = Color.FromArgb(hex);

                return new UserChartItem
                {
                    UserId = money.subUserId,
                    UserName = userName,
                    AvatarUrl = avatar,
                    Amount = (float)money.amountMoney,
                    ChartColor = skColor,
                    BackgroundColor = mauiColor
                };
            }).ToList();

            UserMoneyPieChart = new PieChart
            {
                Entries = items.Select(i => new ChartEntry(i.Amount)
                {
                    Label = "",
                    ValueLabel = "",
                    Color = i.ChartColor
                }).ToList(),
                LabelTextSize = 0,
                BackgroundColor = SKColors.Transparent
            };

            UserChartItems.Clear();
            foreach (var it in items)
                UserChartItems.Add(it);
        }

        private string RandomColor()
            => new Random().Next(0x1000000).ToString("X6");

        public async Task LoadMonthlyMoneyUsedAnalyticsAsync()
        {
            MonthlyUsageChart = null;

            var userId = await SecureStorage.GetAsync("user_id");
            var list = await _moneyUsedApiClient.GetMoneyUsedAsync(userId);

            if (list == null || !list.Any())
            {
                MonthlyUsageChart = new LineChart { Entries = new[] { new ChartEntry(0) { Label = "", ValueLabel = "0" } }, BackgroundColor = SKColors.Transparent };
                return;
            }

            DateTime startDate;
            DateTime endDate = DateTime.Now;

            if (ShowYearlyAnalytics)
            {
                startDate = new DateTime(endDate.Year, 1, 1);
                MonthlyUsageChartTitle = "Витрати за " + endDate.Year;
            }
            else if (ShowMonthlyAnalytics)
            {
                startDate = new DateTime(endDate.Year, endDate.Month, 1);
                MonthlyUsageChartTitle = "Витрати за " + endDate.ToString("MMMM", CultureInfo.CurrentCulture);
            }
            else if (ShowWeeklyAnalytics)
            {
                startDate = endDate.AddDays(-(int)endDate.DayOfWeek + (int)DayOfWeek.Monday);
                MonthlyUsageChartTitle = "Витрати за тиждень (" + startDate.ToString("dd.MM") + " - " + endDate.ToString("dd.MM") + ")";
            }
            else
            {
                // За замовчуванням - місяць (щоб уникнути неочікуваної поведінки)
                startDate = new DateTime(endDate.Year, endDate.Month, 1);
                MonthlyUsageChartTitle = "Витрати за " + endDate.ToString("MMMM", CultureInfo.CurrentCulture);
                ShowMonthlyAnalytics = true;
            }

            var filteredData = list.Where(d => d.dateTimeUsed >= startDate && d.dateTimeUsed <= endDate).ToList();

            List<ChartEntry> entries = new();

            if (ShowYearlyAnalytics)
            {
                entries = Enumerable.Range(1, 12)
                    .Select(month =>
                    {
                        var totalAmount = filteredData
                            .Where(d => d.dateTimeUsed.Month == month)
                            .Sum(x => x.amountMoney);
                        return new ChartEntry((float)totalAmount)
                        {
                            Label = new DateTime(endDate.Year, month, 1).ToString("MMM", CultureInfo.CurrentCulture),
                            ValueLabel = totalAmount.ToString("F2"),
                            Color = SKColor.Parse("#2c3e50")
                        };
                    }).ToList();
            }
            else if (ShowMonthlyAnalytics)
            {
                int daysInMonth = DateTime.DaysInMonth(endDate.Year, endDate.Month);
                entries = Enumerable.Range(1, daysInMonth)
                    .Select(day =>
                    {
                        var totalAmount = filteredData
                            .Where(d => d.dateTimeUsed.Day == day)
                            .Sum(x => x.amountMoney);
                        return new ChartEntry((float)totalAmount)
                        {
                            Label = new DateTime(endDate.Year, endDate.Month, day).ToString("dd", CultureInfo.CurrentCulture),
                            ValueLabel = totalAmount.ToString("F2"),
                            Color = SKColor.Parse("#2c3e50")
                        };
                    }).ToList();
            }
            // всередині LoadMonthlyMoneyUsedAnalyticsAsync()
            else if (ShowWeeklyAnalytics)
            {
                // різниця між сьогоднішнім днем і понеділком
                int diff = (7 + ((int)endDate.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
                startDate = endDate.AddDays(-diff);
                MonthlyUsageChartTitle = $"Витрати за тиждень ({startDate:dd.MM} - {endDate:dd.MM})";

                entries = Enumerable.Range(0, 7)
                    .Select(offset =>
                    {
                        var date = startDate.AddDays(offset);
                        var sum = filteredData
                            .Where(d => d.dateTimeUsed.Date == date.Date)
                            .Sum(x => x.amountMoney);
                        return new ChartEntry((float)sum)
                        {
                            Label = date.ToString("dd"),
                            ValueLabel = sum.ToString("F2"),
                            Color = SKColor.Parse("#2c3e50")
                        };
                    }).ToList();
            }

            MonthlyUsageChart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Spline,
                PointMode = PointMode.Circle,
                PointSize = 8,
                LabelTextSize = 20,
                BackgroundColor = SKColors.Transparent
            };
        }


        [RelayCommand]
        private async Task SelectMoneyUsersUsedTab()
        {
            SelectedTab = "User";
            CurrentAnalyticsContent = new Label { Text = "Вміст: Витрати за користувачами" };
            OnPropertyChanged(nameof(IsMoneyUsersUsedTabSelected));
            OnPropertyChanged(nameof(IsMoneyUsedTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));

            // Очищаємо графік загальних витрат
            MonthlyUsageChart = null;
            // Переконайтеся, що цей OnPropertyChanged викликається, якщо ви використовуєте його для оновлення UI
            OnPropertyChanged(nameof(MonthlyUsageChart));

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
        private async Task SelectMoneyUsedTab()
        {
            SelectedTab = "Token";
            OnPropertyChanged(nameof(IsMoneyUsedTabSelected));
            OnPropertyChanged(nameof(IsMoneyUsersUsedTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));

            // 1) Скидаємо попередній графік…
            MonthlyUsageChart = null;
            OnPropertyChanged(nameof(MonthlyUsageChart));

            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // 2) Завантажуємо нові дані й відрендеримо чистий графік
                await LoadMonthlyMoneyUsedAnalyticsAsync();
            }
            finally { IsBusy = false; }
        }


        [RelayCommand]
        private async Task SelectPaymentTabAsync()
        {
            SelectedTab = "Payment";
            CurrentAnalyticsContent = new Label { Text = "Вміст: Історія оплати" };
            OnPropertyChanged(nameof(IsMoneyUsersUsedTabSelected));
            OnPropertyChanged(nameof(IsMoneyUsedTabSelected));
            OnPropertyChanged(nameof(IsPaymentTabSelected));

            // Очищаємо обидва графіки, оскільки вкладка "Історія оплати" не відображає їх
            UserMoneyPieChart = null;
            OnPropertyChanged(nameof(UserMoneyPieChart));
            MonthlyUsageChart = null;
            OnPropertyChanged(nameof(MonthlyUsageChart));

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
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

    // Розширений клас для прив'язки кольору в UI і для графіка
    public class UserChartItem
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public float Amount { get; set; }
        public string ValueLabel => Amount.ToString("F2");

        // Колір для Microcharts:
        public SKColor ChartColor { get; set; }

        // Колір для MAUI UI:
        public Color BackgroundColor { get; set; }
    }

    // Конвертер SKColor → Microsoft.Maui.Graphics.Color
    public class SKColorToMauiColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SKColor sk)
            {
                return Color.FromArgb($"#{sk.Red:X2}{sk.Green:X2}{sk.Blue:X2}");
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}