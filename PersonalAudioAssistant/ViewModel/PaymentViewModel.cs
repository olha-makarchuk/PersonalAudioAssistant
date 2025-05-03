using Android.Text;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Model.Payment;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using PersonalAudioAssistant.Views;
using PersonalAudioAssistant.Views.History;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static Com.Google.Android.Exoplayer2.Upstream.Experimental.SlidingWeightedAverageBandwidthStatistic;

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

        [ObservableProperty]
        private string audioRequestPath;

        [ObservableProperty]
        private string audioAnswerPath;

        [ObservableProperty]
        private bool isAnswerPathAvailable;

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

        public ObservableCollection<ExamplesPaymentResponse> Examples { get; } = new();

        [ObservableProperty]
        private bool isExampleSelected;

        public async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await LoadPaymentInfo();
                await LoadExamplesAsync();
            }
            finally { IsBusy = false; }
        }

        private async Task LoadExamplesAsync()
        {
            var examples = await new ExamplesPayment().GetExamplesPayment();
            Examples.Clear();
            foreach (var ex in examples)
                Examples.Add(ex);
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

        [ObservableProperty]
        private bool isExample1Selected;

        [ObservableProperty]
        private bool isExample2Selected;

        [ObservableProperty]
        private bool isExample3Selected;

        partial void OnIsExample1SelectedChanged(bool value)
        {
            if (value)
            {
                AudioRequestPath = null;
                AudioAnswerPath = null;
                IsAnswerPathAvailable = false;
                IsExample2Selected = false;
                IsExample3Selected = false;

                GetExamplesPayment(0);
            }
        }

        partial void OnIsExample2SelectedChanged(bool value)
        {
            if (value)
            {
                AudioRequestPath = null;
                AudioAnswerPath = null;
                IsAnswerPathAvailable = false;
                IsExample1Selected = false;
                IsExample3Selected = false;

                GetExamplesPayment(1);
            }
        }

        partial void OnIsExample3SelectedChanged(bool value)
        {
            if (value)
            {
                AudioRequestPath = null;
                AudioAnswerPath = null;
                IsAnswerPathAvailable = false;
                IsExample1Selected = false;
                IsExample2Selected = false;

                GetExamplesPayment(2);
            }
        }

        private async Task GetExamplesPayment(int numberExample)
        {
            ExamplesPayment examplesPayment = new ExamplesPayment();
            var examplesList = await examplesPayment.GetExamplesPayment();

            var example = examplesList[numberExample];
            TextInput = example.textRequest;

            var totalCost = example.transcriptionCost + example.inputCost + example.outputCost + example.ttsCost;
            var howManyRequestsFiveDollars = (5 / totalCost);  

            var summary = $"""
                🧠 Відповідь: "{example.textAnswer}"
                🎙️ Тривалість транскрипції: {example.audioRequestDuration:F1} сек → ${example.transcriptionCost:F5}
                🤖 Вартість обробки запиту GPT: {example.inputTokens} токенів → ${example.inputCost:F5}
                📤 Вартість відповіді GPT: {example.outputTokens} токенів → ${example.outputCost:F5}
                🗣️ Вартість озвучення тексту: {example.charCount} символів → ${example.ttsCost:F5}
                💰 Загальна вартість: ${totalCost:F5}
                💸 За $5 можна зробити приблизно: {howManyRequestsFiveDollars:F0} подібних запитів
                """;
            
            IsResultExist = true;
            IsAnswerPathAvailable = true;
            TokenCalculationResult = summary;
            AudioRequestPath = example.audioRequestPath;
            AudioAnswerPath = example.audioAnswerPath;
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
            🧠 Відповідь: "{answer}"
            🎙️ Тривалість транскрипції: {durationInSeconds:F1} сек → ${transcriptionCost:F5}
            🤖 Вартість обробки запиту GPT: {inputTokenCount} токенів → ${gptInCost:F5}
            📤 Вартість відповіді GPT: {outputTokenCount} токенів → ${gptOutCost:F5}
            🗣️ Вартість озвучення тексту: {charCount} символів → ${ttsCost:F5}
            💰 Загальна вартість: ${totalCost:F5}
            💸 За $5 можна зробити приблизно: {howManyRequestsFiveDollars:F0} подібних запитів
            """;
            AudioRequestPath = null;
            AudioAnswerPath = null;
            IsAnswerPathAvailable = false;
            IsResultExist = true;
            TokenCalculationResult = summary;
        }



        [ObservableProperty]
        private bool canPlayRequest;

        [ObservableProperty]
        private bool canPlayAnswer;

        partial void OnAudioRequestPathChanged(string value)
        {
            CanPlayRequest = !string.IsNullOrEmpty(value);
            PlayRequestCommand.NotifyCanExecuteChanged();
        }

        partial void OnAudioAnswerPathChanged(string value)
        {
            CanPlayAnswer = !string.IsNullOrEmpty(value);
            PlayAnswerCommand.NotifyCanExecuteChanged();
        }


        private bool isPlayingRequest;

        [RelayCommand(CanExecute = nameof(CanPlayRequest))]
        private async Task PlayRequest()
        {
            try 
            { 
                var mediaElement = ((PaymentPage)Shell.Current.CurrentPage).MediaElementRequest;
                mediaElement.Source = AudioRequestPath;

                if (isPlayingRequest)
                {
                    mediaElement.Pause();
                    isPlayingRequest = false;
                }
                else
                {
                    mediaElement.Play();
                    isPlayingRequest = true;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відтворити голос: {ex.Message}", "OK");
            }
        }

        private bool isPlayingAnswer;

        [RelayCommand(CanExecute = nameof(CanPlayRequest))]
        private async Task PlayAnswer()
        {
            try
            {
                var mediaElement = ((PaymentPage)Shell.Current.CurrentPage).MediaElementAnswer;
                mediaElement.Source = AudioAnswerPath;

                if (isPlayingAnswer)
                {
                    mediaElement.Pause();
                    isPlayingAnswer = false;
                }
                else
                {
                    mediaElement.Play();
                    isPlayingAnswer = true;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відтворити голос: {ex.Message}", "OK");
            }
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