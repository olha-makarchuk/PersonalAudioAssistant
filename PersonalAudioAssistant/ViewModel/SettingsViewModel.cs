using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using PersonalAudioAssistant.Services;
using System.Collections.ObjectModel;
using static Android.Content.Res.Resources;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;
        private string UserId;

        [ObservableProperty] private string? theme;
        [ObservableProperty] private string? payment;
        [ObservableProperty] private int minTokenThreshold;
        [ObservableProperty] private int chargeAmount;
        [ObservableProperty] private int balance;
        [ObservableProperty] private string email;
        [ObservableProperty] private ObservableCollection<string> themes;
        [ObservableProperty]
        private bool isBusy;

        public SettingsViewModel(IMediator mediator, AuthTokenManager authTokenManager)
        {
            _mediator = mediator;
            Themes = new ObservableCollection<string> { "Light", "Dark" };
            _authTokenManager = authTokenManager;
            LoadSettingsAsync();
        }

        public string PaymentButtonText => string.IsNullOrWhiteSpace(Payment) ? "Додати карту" : "Змінити карту";

        // Сповіщення про зміну:
        partial void OnPaymentChanged(string? value)
        {
            OnPropertyChanged(nameof(PaymentButtonText));
        }

        public async void LoadSettingsAsync()
        {
            IsBusy = true;
            try
            {
                UserId = await SecureStorage.GetAsync("user_id");

                var settings = await _mediator.Send(new GetSettingsByUserIdQuery()
                {
                    UserId = UserId
                });

                if (settings != null)
                {
                    Theme = settings.Theme;
                    Payment = settings.Payment;
                    MinTokenThreshold = settings.MinTokenThreshold;
                    ChargeAmount = settings.ChargeAmount;
                    Balance = settings.Balance;
                }
                Email = await SecureStorage.GetAsync("user_email");
            }
            finally
            {
                IsBusy = false;
            }
        }


        [RelayCommand]
        public async Task SaveSettingsAsync()
        {
            await _mediator.Send(new UpdateSettingsCommand()
            {
                UserId = UserId,
                Theme = Theme,
                Payment = Payment,
                MinTokenThreshold = MinTokenThreshold,
                ChargeAmount = ChargeAmount
            });
        }

        [RelayCommand]
        public async Task AddCard()
        {
            Payment = "**** **** **** 1234"; 
        }

        [RelayCommand]
        public async Task SignOut()
        {
            _authTokenManager.SignOutAsync();

            await Shell.Current.GoToAsync("//AuthorizationPage");
        }
    }
}
