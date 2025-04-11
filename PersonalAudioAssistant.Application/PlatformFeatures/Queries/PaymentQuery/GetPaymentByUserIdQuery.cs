using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery
{
    public class GetPaymentByUserIdQuery : IRequest<Payment>
    {
        public string? UserId { get; set; }

        public class GetPaymentByUserIdQueryHandler : IRequestHandler<GetPaymentByUserIdQuery, Payment>
        {
            private readonly IPaymentRepository _paymentRepository;
            public GetPaymentByUserIdQueryHandler(IPaymentRepository paymentRepository)
            {
                _paymentRepository = paymentRepository;
            }
            public async Task<Payment> Handle(GetPaymentByUserIdQuery query, CancellationToken cancellationToken)
            {
                var user = await _paymentRepository.GetPaymentByUserIdAsync(query.UserId, cancellationToken);
                if (user == null)
                {
                    throw new Exception("Payment not found");
                }

                return user;
            }
        }
    }
}
