using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.AutoPaymentsQuery;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.Model.Payment;
using PersonalAudioAssistant.Services;
using System.Text.RegularExpressions;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class PaymentViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly ApiClientTokens _apiClientTokens;
        private readonly ManageCacheData _manageCacheData;

        public PaymentViewModel(IMediator mediator, ApiClientTokens apiClientTokens, ManageCacheData manageCacheData)
        {
            _mediator = mediator;
            CardModel = new CardModel();
            AutoPaymentModel = new AutoPaymentModel();
            _apiClientTokens = apiClientTokens;
            _manageCacheData = manageCacheData;
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

            var paymentResult = await _mediator.Send(new GetPaymentByUserIdQuery { UserId = userId });
            CardModel.MaskedCardNumber = paymentResult.MaskedCardNumber;
            CardModel.DateExpirience = paymentResult.DataExpired;
            IsCardPreset = !string.IsNullOrEmpty(CardModel.MaskedCardNumber);

            var autoPaymentResult = await _mediator.Send(new GetAutoPaymentsByUserIdQuery { UserId = userId });
            AutoPaymentModel.IsAutoPaymentEnabled = autoPaymentResult.IsAutoPayment;
            AutoPaymentModel.MinimumTokenBalance = autoPaymentResult.MinTokenThreshold;
            AutoPaymentModel.AutoRechargeAmount = autoPaymentResult.ChargeAmount;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task UpdateCard()
        {
            IsCardPreset = false;
            IsUpdatingCard = true;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task CancelUpdateCard()
        {
            IsCardPreset = true;
            IsUpdatingCard = false;
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

        // --- Команди ---
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


                var random = new Random();
                var fakeToken = Guid.NewGuid().ToString();

                var lastFourDigits = CardModel.CardNumber.Substring(CardModel.CardNumber.Length - 4);

                var fakeCardNumber = $"**** **** **** {lastFourDigits}";

                CardModel.MaskedCardNumber = fakeCardNumber;

                var command = new UpdatePaymentCommand
                {
                    UserId = userId,
                    MaskedCardNumber = fakeCardNumber,
                    PaymentGatewayToken = fakeToken,
                    DataExpiredCard = CardModel.DateExpirience
                };
                await _mediator.Send(command);
                IsCardPreset = true;
            }
            finally
            {
                IsBusy = false;
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

                await _mediator.Send(new UpdatePaymentCommand
                {
                    UserId = userId,
                    MaskedCardNumber = null,
                    PaymentGatewayToken = null,
                    DataExpiredCard = null
                });

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

                await _mediator.Send(new UpdateAutoPaymentCommand
                {
                    UserId = userId,
                    ChargeAmount = AutoPaymentModel.AutoRechargeAmount,
                    IsAutoPayment = AutoPaymentModel.IsAutoPaymentEnabled,
                    MinTokenThreshold = AutoPaymentModel.MinimumTokenBalance,
                });
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

                var command = new UpdateBalanceCommand()
                {
                    RechargeAmountInput = RechargeAmountInput,
                    UserId = userId,
                    MaskedCardNumber = CardModel.MaskedCardNumber,
                    DescriptionPayment = "Поповнення балансу"
                };
                await _mediator.Send(command);
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
                   🧠 Відповідь(приклад): "{answer}"
                   🎙️ Транскрипція: {durationInSeconds:F1} сек → ${transcriptionCost:F5}
                   🤖 GPT Input: {inputTokenCount} токенів → ${gptInCost:F5}
                   📤 GPT Output: {outputTokenCount} токенів → ${gptOutCost:F5}
                   🗣️ Озвучення: {charCount} символів → ${ttsCost:F5}
                   💰 Загалом: ${totalCost:F5}
                   💸 За $5 можна зробити приблизно: {howManyRequestsFiveDollars:F0} схожих запитів
                   """;

            await Shell.Current.DisplayAlert("Оцінка", summary, "OK");
        }
    }
}