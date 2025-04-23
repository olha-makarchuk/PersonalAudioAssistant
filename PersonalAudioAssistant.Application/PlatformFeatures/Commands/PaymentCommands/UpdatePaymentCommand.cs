using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands
{
    public class UpdatePaymentCommand : IRequest
    {
        public required string UserId { get; set; }
        public required string PaymentGatewayToken { get; set; }
        public required string MaskedCardNumber { get; set; }
        public required string DataExpiredCard { get; set; }
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
                var payment = await _paymentRepository.GetPaymentByUserIdAsync(request.UserId, cancellationToken);

                if (payment == null)
                {
                    throw new Exception("Payment not found");
                }

                payment.PaymentGatewayToken = request.PaymentGatewayToken;
                payment.MaskedCardNumber = request.MaskedCardNumber;
                payment.DataExpiredCard = request.DataExpiredCard;

                await _paymentRepository.UpdatePaymentAsync(payment, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving payment information", ex);
            }
        }
    }
}
