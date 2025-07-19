using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands
{
    public class CreatePaymentCommand : IRequest<Unit>
    {
        public required string UserId { get; set; }
    }

    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Unit>
    {
        private readonly IPaymentRepository _paymentRepository;

        public CreatePaymentCommandHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Unit> Handle(CreatePaymentCommand request, CancellationToken cancellationToken = default)
        {
            var payment = new Payment()
            {
                UserId = request.UserId,
                PaymentGatewayToken = string.Empty,
                MaskedCardNumber = string.Empty,
                DataExpiredCard = string.Empty
            };

            await _paymentRepository.AddPaymentAsync(payment, cancellationToken);
            return Unit.Value;
        }
    }
}
