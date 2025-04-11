using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.AutoPaymentsQuery
{
    public class GetAutoPaymentsByUserIdQuery : IRequest<AutoPayments>
    {
        public string? UserId { get; set; }

        public class GetAutoPaymentsByUserIdQueryHandler : IRequestHandler<GetAutoPaymentsByUserIdQuery, AutoPayments>
        {
            private readonly IAutoPaymentRepository _autoPaymentRepository;
            public GetAutoPaymentsByUserIdQueryHandler(IAutoPaymentRepository autoPaymentRepository)
            {
                _autoPaymentRepository = autoPaymentRepository;
            }
            public async Task<AutoPayments> Handle(GetAutoPaymentsByUserIdQuery query, CancellationToken cancellationToken)
            {
                var autoPayment = await _autoPaymentRepository.GetAutoPaymentByUserIdAsync(query.UserId, cancellationToken);
                if (autoPayment == null)
                {
                    throw new Exception("AutoPayment not found");
                }

                return autoPayment;
            }
        }
    }
}
