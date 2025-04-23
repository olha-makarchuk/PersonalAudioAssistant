using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class UpdateBalanceCommand: IRequest
    {
        public required string UserId { get; set; }
        public decimal RechargeAmountInput { get; set; }
        public string MaskedCardNumber { get; set; }
        public string DescriptionPayment { get; set; }
    }

    public class UpdateBalanceCommandHandler : IRequestHandler<UpdateBalanceCommand>
    {
        private readonly IAppSettingsRepository _settingsRepository;
        private readonly IPaymentHistoryRepository _paymentHistoryRepository;

        public UpdateBalanceCommandHandler(IAppSettingsRepository settingsRepository, IPaymentHistoryRepository paymentHistoryRepository)
        {
            _settingsRepository = settingsRepository;
            _paymentHistoryRepository = paymentHistoryRepository;
        }

        public async Task Handle(UpdateBalanceCommand request, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsRepository.GetSettingsByUserIdAsync(request.UserId, cancellationToken);

            if (settings == null)
            {
                throw new Exception("Settings not found.");
            }

            settings.Balance += request.RechargeAmountInput;

            var paymentHistory = new PaymentHistory
            {
                UserId = request.UserId,
                Amount = request.RechargeAmountInput,
                DateTimePayment = DateTime.UtcNow,
                MaskedCardNumber = request.MaskedCardNumber,
                Description = request.DescriptionPayment
            };

            await _paymentHistoryRepository.AddPaymentHistoryAsync(paymentHistory, cancellationToken);
            await _settingsRepository.UpdateSettingsAsync(settings, cancellationToken);
        }
    }
}
