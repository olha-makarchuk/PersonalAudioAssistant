using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands
{
    public class CreateAutoPaymentCommand: IRequest<Unit>
    {
        public required string UserId { get; set; }
    }

    public class CreateAutoPaymentCommandHandler : IRequestHandler<CreateAutoPaymentCommand, Unit>
    {
        private readonly IAutoPaymentRepository _autoPaymentRepository;

        public CreateAutoPaymentCommandHandler(IAutoPaymentRepository autoPaymentRepository)
        {
            _autoPaymentRepository = autoPaymentRepository;
        }

        public async Task<Unit> Handle(CreateAutoPaymentCommand request, CancellationToken cancellationToken = default)
        {
            var autoPayment = new AutoPayments()
            {
                IsAutoPayment = false,
                MinTokenThreshold = 0,
                ChargeAmount = 0,
                UserId = request.UserId
            };

            await _autoPaymentRepository.AddAutoPaymentAsync(autoPayment, cancellationToken);
            return Unit.Value;
        }
    }
}
