using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.MoneyUsed;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.MoneyUsedQuery
{
    public class GetMoneyUsedByMainUserIdQuery : IRequest<List<MoneyUsedResponse>>
    {
        public required string MainUserId { get; set; }

        public class GetMoneyUsedByMainUserIdQueryHandler : IRequestHandler<GetMoneyUsedByMainUserIdQuery, List<MoneyUsedResponse>>
        {
            private readonly IMoneyUsedRepository _moneyUsedRepository;

            public GetMoneyUsedByMainUserIdQueryHandler(IMoneyUsedRepository moneyUsedRepository)
            {
                _moneyUsedRepository = moneyUsedRepository;
            }

            public async Task<List<MoneyUsedResponse>> Handle(GetMoneyUsedByMainUserIdQuery query, CancellationToken cancellationToken)
            {
                var entities = await _moneyUsedRepository.GetMoneyUsedByMainUserIdAsync(query.MainUserId, cancellationToken);

                if (entities == null || entities.Count == 0)
                {
                    throw new Exception("MoneyUsed records not found.");
                }

                var responseList = entities.Select(entity => new MoneyUsedResponse
                {
                    Id = entity.Id.ToString(),
                    MainUserId = entity.MainUserId,
                    AmountMoney = entity.AmountMoney,
                    DateTimeUsed = entity.DateTimeUsed
                }).ToList();

                return responseList;
            }
        }
    }
}
