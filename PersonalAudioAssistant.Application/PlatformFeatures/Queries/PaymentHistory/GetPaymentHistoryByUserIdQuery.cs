using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery;
using PersonalAudioAssistant.Contracts.Payment;
using PersonalAudioAssistant.Contracts.PaymentHistory;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentHistory
{
    public class GetPaymentHistoryByUserIdQuery: IRequest<List<PaymentHistoryResponse>>
    {
        public required string UserId { get; set; }

        public class GetPaymentHistoryByUserIdQueryHandler : IRequestHandler<GetPaymentHistoryByUserIdQuery, List<PaymentHistoryResponse>>
        {
            private readonly IPaymentHistoryRepository _paymentHistoryRepository;

            public GetPaymentHistoryByUserIdQueryHandler(IPaymentHistoryRepository paymentHistoryRepository)
            {
                _paymentHistoryRepository = paymentHistoryRepository;
            }

            public async Task<List<PaymentHistoryResponse>> Handle(GetPaymentHistoryByUserIdQuery query, CancellationToken cancellationToken)
            {
                var paymentHistoryExist = await _paymentHistoryRepository.GetPaymentsHistoryByUserIdAsync(query.UserId, cancellationToken);

                if (paymentHistoryExist == null)
                {
                    throw new Exception("Payment not found");
                }

                var responseList = paymentHistoryExist.Select(history => new PaymentHistoryResponse
                {
                    DataTimePayment = history.DateTimePayment,
                    Description = history.Description,
                    MaskedCardNumber = history.MaskedCardNumber,
                    Amount = history.Amount
                }).ToList();

                return responseList;
            }
        }
    }
}
