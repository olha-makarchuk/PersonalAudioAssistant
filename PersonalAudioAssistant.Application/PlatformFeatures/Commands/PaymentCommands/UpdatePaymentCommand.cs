using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands
{
    public class UpdatePaymentCommand : IRequest
    {
        public required string UserId { get; set; }
        public required string PaymentGatewayToken { get; set; }
        public required string MaskedCardNumber { get; set; }
    }

    public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand>
    {
        private readonly IPaymentRepository _paymentRepository;

        public UpdatePaymentCommandHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var autoPayment = await _paymentRepository.GetPaymentByUserIdAsync(request.UserId, cancellationToken);

                if (autoPayment == null)
                {
                    throw new Exception("Payment not found");
                }

                autoPayment.PaymentGatewayToken = request.PaymentGatewayToken;
                autoPayment.MaskedCardNumber = request.MaskedCardNumber;

                await _paymentRepository.UpdatePaymentAsync(autoPayment, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving payment information", ex);
            }
        }
    }
}
