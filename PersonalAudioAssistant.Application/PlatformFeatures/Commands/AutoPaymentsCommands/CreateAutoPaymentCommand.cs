using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands
{
    public class CreateAutoPaymentCommand: IRequest
    {
        public required string UserId { get; set; }
    }

    public class CreateAutoPaymentCommandHandler : IRequestHandler<CreateAutoPaymentCommand>
    {
        private readonly IAutoPaymentRepository _autoPaymentRepository;

        public CreateAutoPaymentCommandHandler(IAutoPaymentRepository autoPaymentRepository)
        {
            _autoPaymentRepository = autoPaymentRepository;
        }

        public async Task Handle(CreateAutoPaymentCommand request, CancellationToken cancellationToken = default)
        {
            var autoPayment = new AutoPayments()
            {
                IsAutoPayment = false,
                MinTokenThreshold = 0,
                ChargeAmount = 0,
                UserId = request.UserId
            };

            await _autoPaymentRepository.AddAutoPaymentAsync(autoPayment, cancellationToken);
        }
    }
}
