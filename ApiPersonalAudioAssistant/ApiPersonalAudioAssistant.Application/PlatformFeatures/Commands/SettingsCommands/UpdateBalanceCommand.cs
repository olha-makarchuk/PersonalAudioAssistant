using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class UpdateBalanceCommand : IRequest<Unit>
    {
        public required string UserId { get; set; }
        public decimal RechargeAmountInput { get; set; }
        public string MaskedCardNumber { get; set; } = string.Empty;
        public string DescriptionPayment { get; set; } = string.Empty;
    }

    public class UpdateBalanceCommandHandler : IRequestHandler<UpdateBalanceCommand, Unit>
    {
        private readonly IAppSettingsRepository _settingsRepository;
        private readonly IPaymentHistoryRepository _paymentHistoryRepository;
        private readonly IAutoPaymentRepository _autoPaymentRepository;

        public UpdateBalanceCommandHandler(
            IAppSettingsRepository settingsRepository,
            IPaymentHistoryRepository paymentHistoryRepository,
            IAutoPaymentRepository autoPaymentRepository)
        {
            _settingsRepository = settingsRepository;
            _paymentHistoryRepository = paymentHistoryRepository;
            _autoPaymentRepository = autoPaymentRepository;
        }

        public async Task<Unit> Handle(UpdateBalanceCommand request, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsRepository
                .GetSettingsByUserIdAsync(request.UserId, cancellationToken);
            if (settings == null)
                throw new Exception("Settings not found.");

            var autoPayment = await _autoPaymentRepository
                .GetAutoPaymentByUserIdAsync(request.UserId, cancellationToken);

            decimal input = request.RechargeAmountInput;

            if (input >= 0)
            {
                // Поповнення балансу
                settings.Balance += input;

                var depositHistory = new PaymentHistory
                {
                    UserId = request.UserId,
                    Amount = input,
                    DateTimePayment = DateTime.UtcNow,
                    MaskedCardNumber = request.MaskedCardNumber,
                    Description = request.DescriptionPayment
                };

                await _paymentHistoryRepository.AddPaymentHistoryAsync(depositHistory, cancellationToken);
            }
            else
            {
                decimal withdrawAmount = Math.Abs(input);

                if (settings.Balance >= withdrawAmount)
                {
                    settings.Balance -= withdrawAmount;
                }
                else
                {
                    // Недостатньо коштів – перевірка автоплатежу
                    if (autoPayment != null && autoPayment.IsAutoPayment)
                    {
                        settings.Balance += autoPayment.ChargeAmount;

                        var autoHistory = new PaymentHistory
                        {
                            UserId = request.UserId,
                            Amount = autoPayment.ChargeAmount,
                            DateTimePayment = DateTime.UtcNow,
                            MaskedCardNumber = request.MaskedCardNumber,
                            Description = "Автопоповнення"
                        };

                        await _paymentHistoryRepository.AddPaymentHistoryAsync(autoHistory, cancellationToken);
                    }

                    if (settings.Balance < withdrawAmount)
                        throw new Exception("Недостатньо коштів навіть після автопоповнення.");

                    settings.Balance -= withdrawAmount;
                }

                // ДОДАТКОВА ПЕРЕВІРКА — баланс після зняття < нижньої межі
                if (autoPayment != null && autoPayment.IsAutoPayment && settings.Balance < autoPayment.MinTokenThreshold)
                {
                    settings.Balance += autoPayment.ChargeAmount;

                    var autoLowBoundHistory = new PaymentHistory
                    {
                        UserId = request.UserId,
                        Amount = autoPayment.ChargeAmount,
                        DateTimePayment = DateTime.UtcNow,
                        MaskedCardNumber = request.MaskedCardNumber,
                        Description = "Автопоповнення"
                    };

                    await _paymentHistoryRepository.AddPaymentHistoryAsync(autoLowBoundHistory, cancellationToken);
                }
            }

            await _settingsRepository.UpdateSettingsAsync(settings, cancellationToken);

            return Unit.Value;
        }
    }
}
