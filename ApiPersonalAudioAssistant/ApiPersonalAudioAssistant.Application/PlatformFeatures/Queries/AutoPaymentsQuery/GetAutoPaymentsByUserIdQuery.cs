using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.AutoPayment;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.AutoPaymentsQuery
{
    public class GetAutoPaymentsByUserIdQuery : IRequest<AutoPaymentResponse>
    {
        public required string UserId { get; set; }

        public class GetAutoPaymentsByUserIdQueryHandler : IRequestHandler<GetAutoPaymentsByUserIdQuery, AutoPaymentResponse>
        {
            private readonly IAutoPaymentRepository _autoPaymentRepository;
            public GetAutoPaymentsByUserIdQueryHandler(IAutoPaymentRepository autoPaymentRepository)
            {
                _autoPaymentRepository = autoPaymentRepository;
            }
            public async Task<AutoPaymentResponse> Handle(GetAutoPaymentsByUserIdQuery query, CancellationToken cancellationToken)
            {
                var autoPayment = await _autoPaymentRepository.GetAutoPaymentByUserIdAsync(query.UserId, cancellationToken);
                if (autoPayment == null)
                {
                    throw new Exception("AutoPayment not found");
                }

                var autoPaymentResponse = new AutoPaymentResponse
                {
                    Id = autoPayment.Id.ToString(),
                    IsAutoPayment = autoPayment.IsAutoPayment,
                    MinTokenThreshold = autoPayment.MinTokenThreshold,
                    ChargeAmount = autoPayment.ChargeAmount,
                    UserId = autoPayment.UserId
                };

                return autoPaymentResponse;
            }
        }
    }
}