using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.Payment;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery
{
    public class GetPaymentByUserIdQuery : IRequest<PaymentResponse>
    {
        public required string UserId { get; set; }

        public class GetPaymentByUserIdQueryHandler : IRequestHandler<GetPaymentByUserIdQuery, PaymentResponse>
        {
            private readonly IPaymentRepository _paymentRepository;

            public GetPaymentByUserIdQueryHandler(IPaymentRepository paymentRepository)
            {
                _paymentRepository = paymentRepository;
            }

            public async Task<PaymentResponse> Handle(GetPaymentByUserIdQuery query, CancellationToken cancellationToken)
            {
                var user = await _paymentRepository.GetPaymentByUserIdAsync(query.UserId, cancellationToken);
                if (user == null)
                {
                    throw new Exception("Payment not found");
                }

                var paymentResponse = new PaymentResponse
                {
                    Id = user.Id.ToString(),
                    UserId = user.UserId,
                    MaskedCardNumber = user.MaskedCardNumber,
                    PaymentGatewayToken = user.PaymentGatewayToken,
                    DataExpired = user.DataExpiredCard,
                };

                return paymentResponse;
            }
        }
    }
}
