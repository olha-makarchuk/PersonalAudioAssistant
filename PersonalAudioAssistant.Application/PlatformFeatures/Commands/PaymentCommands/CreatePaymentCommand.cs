using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands
{
    public class CreatePaymentCommand : IRequest
    {
        public required string UserId { get; set; }
    }

    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand>
    {
        private readonly IPaymentRepository _paymentRepository;

        public CreatePaymentCommandHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task Handle(CreatePaymentCommand request, CancellationToken cancellationToken = default)
        {
            var payment = new Payment()
            {
                UserId = request.UserId,
                PaymentGatewayToken = string.Empty,
                MaskedCardNumber = string.Empty,
                DataExpiredCard = string.Empty
            };

            await _paymentRepository.AddPaymentAsync(payment, cancellationToken);
        }
    }
}
