using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.AutoPaymentsQuery;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class PaymentViewModel : ObservableObject
    {
        private readonly IMediator _mediator;

        public PaymentViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- Busy state ---
        [ObservableProperty]
        private bool isBusy;

        public bool IsNotBusy => !IsBusy;

        partial void OnIsBusyChanged(bool value)
        {
            // Повідомляємо UI, що змінилося IsNotBusy
            OnPropertyChanged(nameof(IsNotBusy));
            // Оновлюємо стан команд
            LiqPayCommand.NotifyCanExecuteChanged();
            DeleteCardCommand.NotifyCanExecuteChanged();
            SaveAutoPaymentSettingsCommand.NotifyCanExecuteChanged();
        }

        // --- Дані картки ---
        [ObservableProperty]
        private string maskedCardNumber;

        [ObservableProperty]
        private string paymentGatewayToken;

        public bool HasCard => !string.IsNullOrWhiteSpace(PaymentGatewayToken) && PaymentGatewayToken != "null";
        public string CardButtonText => HasCard ? "Змінити карту" : "Додати карту";
        public bool IsCardPresent => HasCard;

        // --- Налаштування автоплатежів ---
        [ObservableProperty]
        private bool isAutoPaymentEnabled;

        [ObservableProperty]
        private int minimumTokenBalance;

        [ObservableProperty]
        private int autoRechargeAmount;

        // --- Ініціалізація ---
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
            PaymentGatewayToken = paymentResult.PaymentGatewayToken;

            var autoPaymentResult = await _mediator.Send(new GetAutoPaymentsByUserIdQuery { UserId = userId });
            IsAutoPaymentEnabled = autoPaymentResult.IsAutoPayment;
            MinimumTokenBalance = autoPaymentResult.MinTokenThreshold;
            AutoRechargeAmount = autoPaymentResult.ChargeAmount;

            OnPropertyChanged(nameof(HasCard));
            OnPropertyChanged(nameof(CardButtonText));
            OnPropertyChanged(nameof(IsCardPresent));
        }

        // --- Команди ---
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task LiqPay()
        {
            IsBusy = true;
            try
            {
                var userId = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userId))
                    return;

                var random = new Random();
                var fakeToken = Guid.NewGuid().ToString();
                var fakeCardNumber = $"**** **** **** {random.Next(1000, 9999)}";

                MaskedCardNumber = fakeCardNumber;
                PaymentGatewayToken = fakeToken; // Update the PaymentGatewayToken

                var command = new UpdatePaymentCommand
                {
                    UserId = userId,
                    MaskedCardNumber = fakeCardNumber,
                    PaymentGatewayToken = fakeToken
                };
                await _mediator.Send(command);

                // Explicitly notify the UI that these properties have changed
                OnPropertyChanged(nameof(HasCard));
                OnPropertyChanged(nameof(CardButtonText));
                OnPropertyChanged(nameof(IsCardPresent));
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
                    PaymentGatewayToken = null
                });

                MaskedCardNumber = null;
                PaymentGatewayToken = null;

                OnPropertyChanged(nameof(HasCard));
                OnPropertyChanged(nameof(CardButtonText));
                OnPropertyChanged(nameof(IsCardPresent));
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
    }
}
