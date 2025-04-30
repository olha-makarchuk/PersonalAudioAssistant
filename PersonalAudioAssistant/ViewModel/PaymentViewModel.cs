using Android.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Model.Payment;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using System.Text.RegularExpressions;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class PaymentViewModel : ObservableObject
    {
        private string _backupCardNumber;
        private string _backupDateExpirience;
        private string _backupCvv;

        private readonly IMediator _mediator;
        private readonly ApiClientTokens _apiClientTokens;
        private readonly ManageCacheData _manageCacheData;
        private AppSettingsApiClient _appSettingsApiClient;
        private PaymentApiClient _paymentApiClient;
        private AutoPaymentApiClient _autoPaymentApiClient;

        [ObservableProperty]
        private string tokenCalculationResult;

        [ObservableProperty]
        private bool isResultExist;

        public PaymentViewModel(IMediator mediator, ApiClientTokens apiClientTokens, ManageCacheData manageCacheData, AppSettingsApiClient appSettingsApiClient, PaymentApiClient paymentApiClient, AutoPaymentApiClient autoPaymentApiClient)
        {
            _appSettingsApiClient = appSettingsApiClient;
            _mediator = mediator;
            CardModel = new CardModel();
            AutoPaymentModel = new AutoPaymentModel();
            _apiClientTokens = apiClientTokens;
            _manageCacheData = manageCacheData;
            _paymentApiClient = paymentApiClient;
            _autoPaymentApiClient = autoPaymentApiClient;
        }

        [ObservableProperty]
        private bool isBusy;
        public CardModel CardModel { get; }
        public AutoPaymentModel AutoPaymentModel { get; }

        public bool IsNotBusy => !IsBusy;

        partial void OnIsBusyChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotBusy));

            DeleteCardCommand.NotifyCanExecuteChanged();
            SaveAutoPaymentSettingsCommand.NotifyCanExecuteChanged();
            RechargeBalanceCommand.NotifyCanExecuteChanged(); 
        }

        [ObservableProperty]
        private bool isCardPreset;

        [ObservableProperty]
        private bool isUpdatingCard;

        private string paymentGatewayToken;

        // --- Поповнення балансу ---
        [ObservableProperty]
        private decimal rechargeAmountInput;

        [ObservableProperty]
        private string textInput;

        public async Task InitializeAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                await LoadPaymentInfo();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadPaymentInfo()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userId))
                return;

            var paymentResult= await _paymentApiClient.GetPaymentByUserIdAsync(userId);

            CardModel.MaskedCardNumber = paymentResult.maskedCardNumber;
            CardModel.DateExpirience = paymentResult.dataExpired;
            IsCardPreset = !string.IsNullOrEmpty(CardModel.MaskedCardNumber);

            var autoPaymentResult = await _autoPaymentApiClient.GetAutoPaymentsByUserIdAsync(userId);
            AutoPaymentModel.IsAutoPaymentEnabled = autoPaymentResult.isAutoPayment;
            AutoPaymentModel.MinimumTokenBalance = autoPaymentResult.minTokenThreshold;
            AutoPaymentModel.AutoRechargeAmount = autoPaymentResult.chargeAmount;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task UpdateCard()
        {
            // зберігаємо старі значення
            _backupCardNumber = CardModel.CardNumber;
            _backupDateExpirience = CardModel.DateExpirience;
            _backupCvv = CardModel.CVV_number;

            // переключаємося в режим введення
            IsCardPreset = false;
            IsUpdatingCard = true;

            // очищаємо поля форми
            CardModel.CardNumber = string.Empty;
            CardModel.DateExpirience = string.Empty;
            CardModel.CVV_number = string.Empty;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task CancelUpdateCard()
        {
            // повертаємо старі значення
            CardModel.CardNumber = _backupCardNumber;
            CardModel.DateExpirience = _backupDateExpirience;
            CardModel.CVV_number = _backupCvv;

            // повертаємося до перегляду картки
            IsCardPreset = true;
            IsUpdatingCard = false;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task AddCard()
        {
            IsBusy = true;
            try
            {
                await Validate();

                var userId = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userId))
                    return;

                // формуємо токен та маску
                var fakeToken = Guid.NewGuid().ToString();
                var last4 = CardModel.CardNumber[^4..];
                var cardMask = $"**** **** **** {last4}";

                await _paymentApiClient.UpdatePaymentAsync(userId, fakeToken, cardMask, CardModel.DateExpirience);

                // оновлюємо модель і повертаємося в режим перегляду
                CardModel.MaskedCardNumber = cardMask;
                IsCardPreset = true;
                IsUpdatingCard = false;

                // очищуємо кеш-резерви,
                // щоб нове натискання «Змінити» починалося з чистих полів
                _backupCardNumber = string.Empty;
                _backupDateExpirience = string.Empty;
                _backupCvv = string.Empty;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task Validate()
        {
            // Валідація полів
            if (!Regex.IsMatch(CardModel.CardNumber ?? string.Empty, @"^\d{16}$"))
            {
                await Shell.Current.DisplayAlert("Помилка", "Невірний формат номера картки. Має бути 16 цифр.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CardModel.DateExpirience) || !Regex.IsMatch(CardModel.DateExpirience, @"^(0[1-9]|1[0-2])\/\d{2}$"))
            {
                await Shell.Current.DisplayAlert("Помилка", "Невірний формат терміну дії. Формат MM/YY.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CardModel.CVV_number) || !Regex.IsMatch(CardModel.CVV_number, "^\\d{3,4}$"))
            {
                await Shell.Current.DisplayAlert("Помилка", "CVV має містити 3 або 4 цифри.", "OK");
                return;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task DeleteCard()
        {
            IsBusy = true;
            try
            {
                await Validate();

                // Запит на підтвердження
                var confirm = await Shell.Current.DisplayAlert(
                    "Підтвердження",
                    "Ви впевнені, що хочете видалити карту?",
                    "Так",
                    "Скасувати"
                );

                if (!confirm)
                    return;

                var userId = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userId))
                    return;

                await _paymentApiClient.UpdatePaymentAsync(userId, null, null, null);

                CardModel.MaskedCardNumber = null;
                IsCardPreset = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task SaveAutoPaymentSettings()
        {
            IsBusy = true;
            try
            {
                var userId = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userId))
                    return;

                await _autoPaymentApiClient.UpdateAutoPaymentAsync(userId, AutoPaymentModel.MinimumTokenBalance, AutoPaymentModel.AutoRechargeAmount, AutoPaymentModel.IsAutoPaymentEnabled);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task RechargeBalance()
        {
            if (RechargeAmountInput <= 0)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, введіть суму для поповнення.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var userId = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userId))
                    return;

                await _appSettingsApiClient.UpdateBalanceAsync(userId, RechargeAmountInput, CardModel.MaskedCardNumber, "Поповнення балансу");
                
                await Shell.Current.DisplayAlert("Успіх", $"Баланс успішно поповнено на {RechargeAmountInput} $", "OK"); 

                await _manageCacheData.UpdateAppSetttingsList();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task CalculatePrice()
        {
            var answer = "Привіт, чим я можу вам допомогти?";
            const double DollarPerSystemToken = 0.0003;

            var inputTokenCount = await _apiClientTokens.GetTokenCountAsync(TextInput);
            var outputTokenCount = await _apiClientTokens.GetTokenCountAsync(answer);

            var wordCount = Regex.Matches(TextInput, @"\b\w+\b").Count;
            var durationInSeconds = wordCount * 0.4;

            var transcriptionCost = (durationInSeconds / 60.0) * 0.006;
            var transcribionOut = await _apiClientTokens.GetTokenCountAsync(TextInput) * 0.00001;

            var gptInCost = inputTokenCount * 0.000002;
            var gptOutCost = outputTokenCount * 0.000008;

            var charCount = answer.Length;
            var ttsCost = (charCount / 1000.0) * 0.0833;

            var totalCost = transcriptionCost + transcribionOut + gptInCost + gptOutCost + ttsCost;
            var tokenCost = totalCost / DollarPerSystemToken;

            // Розрахунок скільки запитів можна здійснити за $5
            var howManyRequestsFiveDollars = (5 / totalCost);  // Кількість запитів за $5

            var summary = $"""
            🧠 Ваша відповідь: "{answer}"
            🎙️ Тривалість транскрипції: {durationInSeconds:F1} сек → ${transcriptionCost:F5}
            🤖 Вартість обробки запиту GPT: {inputTokenCount} токенів → ${gptInCost:F5}
            📤 Вартість відповіді GPT: {outputTokenCount} токенів → ${gptOutCost:F5}
            🗣️ Вартість озвучення тексту: {charCount} символів → ${ttsCost:F5}
            💰 Загальна вартість: ${totalCost:F5}
            💸 За $5 можна зробити приблизно: {howManyRequestsFiveDollars:F0} подібних запитів
            """;

            IsResultExist = true;
            TokenCalculationResult = summary;
        }

        public void OnNavigatedFrom()
        {
            CardModel.CardNumber = string.Empty;
            CardModel.DateExpirience = string.Empty;
            CardModel.CVV_number = string.Empty;
            CardModel.MaskedCardNumber = string.Empty;

            AutoPaymentModel.IsAutoPaymentEnabled = false;
            AutoPaymentModel.MinimumTokenBalance = 0;
            AutoPaymentModel.AutoRechargeAmount = 0;

            RechargeAmountInput = 0;
            TextInput = string.Empty;

            IsResultExist = false;
            IsCardPreset = false;
            IsUpdatingCard = false;
            paymentGatewayToken = null;

            TokenCalculationResult = string.Empty;
        }
    }
}