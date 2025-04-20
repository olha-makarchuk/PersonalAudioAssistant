using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.AutoPaymentsQuery;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery;
using System.Text.RegularExpressions;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class PaymentViewModel : ObservableObject
    {
        private readonly IMediator _mediator;

        public PaymentViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [ObservableProperty]
        private bool isBusy;

        public bool IsNotBusy => !IsBusy;

        partial void OnIsBusyChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotBusy));

            DeleteCardCommand.NotifyCanExecuteChanged();
            SaveAutoPaymentSettingsCommand.NotifyCanExecuteChanged();
            RechargeBalanceCommand.NotifyCanExecuteChanged(); 
        }

        [ObservableProperty]
        private string maskedCardNumber;

        [ObservableProperty]
        public string cardNumber;

        [ObservableProperty]
        private string cVV_number;

        [ObservableProperty]
        private string dateExpirience;

        [ObservableProperty]
        private bool isCardPreset;

        [ObservableProperty]
        private bool isUpdatingCard;

        private string paymentGatewayToken;

        // --- Налаштування автоплатежів ---
        [ObservableProperty]
        private bool isAutoPaymentEnabled;

        [ObservableProperty]
        private int minimumTokenBalance;

        [ObservableProperty]
        private int autoRechargeAmount;

        // --- Поповнення балансу ---
        [ObservableProperty]
        private decimal rechargeAmountInput;

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
            MaskedCardNumber = paymentResult.MaskedCardNumber;
            DateExpirience = paymentResult.DataExpired;
            IsCardPreset = !string.IsNullOrEmpty(MaskedCardNumber);

            var autoPaymentResult = await _mediator.Send(new GetAutoPaymentsByUserIdQuery { UserId = userId });
            IsAutoPaymentEnabled = autoPaymentResult.IsAutoPayment;
            MinimumTokenBalance = autoPaymentResult.MinTokenThreshold;
            AutoRechargeAmount = autoPaymentResult.ChargeAmount;
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
            if (!Regex.IsMatch(CardNumber ?? string.Empty, @"^\d{16}$"))
            {
                await Shell.Current.DisplayAlert("Помилка", "Невірний формат номера картки. Має бути 16 цифр.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(DateExpirience) || !Regex.IsMatch(DateExpirience, @"^(0[1-9]|1[0-2])\/\d{2}$"))
            {
                await Shell.Current.DisplayAlert("Помилка", "Невірний формат терміну дії. Формат MM/YY.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CVV_number) || !Regex.IsMatch(CVV_number, "^\\d{3,4}$"))
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

                var lastFourDigits = CardNumber.Substring(CardNumber.Length - 4);

                var fakeCardNumber = $"**** **** **** {lastFourDigits}";

                MaskedCardNumber = fakeCardNumber;

                var command = new UpdatePaymentCommand
                {
                    UserId = userId,
                    MaskedCardNumber = fakeCardNumber,
                    PaymentGatewayToken = fakeToken,
                    DataExpired = DateExpirience
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
                    DataExpired = null
                });

                MaskedCardNumber = null;
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
                    ChargeAmount = AutoRechargeAmount,
                    IsAutoPayment = IsAutoPaymentEnabled,
                    MinTokenThreshold = MinimumTokenBalance,
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

                RechargeAmountInput = 5;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}