using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.MoneyUsersUsed;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.MoneyUsersUsedQuery
{
    public class GetMoneyUsersUsedByMainUserIdQuery : IRequest<List<MoneyUsersUsedResponse>>
    {
        public required string MainUserId { get; set; }

        public class GetMoneyUsersUsedByMainUserIdQueryHandler : IRequestHandler<GetMoneyUsersUsedByMainUserIdQuery, List<MoneyUsersUsedResponse>>
        {
            private readonly IMoneyUsersUsedRepository _moneyUsersUsedRepository;
            private readonly ISubUserRepository _subUserRepository;

            public GetMoneyUsersUsedByMainUserIdQueryHandler(IMoneyUsersUsedRepository moneyUsersUsedRepository, ISubUserRepository subUserRepository)
            {
                _moneyUsersUsedRepository = moneyUsersUsedRepository;
                _subUserRepository = subUserRepository;
            }

            public async Task<List<MoneyUsersUsedResponse>> Handle(GetMoneyUsersUsedByMainUserIdQuery query, CancellationToken cancellationToken)
            {
                var users = await _subUserRepository.GetAllUsersByUserId(query.MainUserId, cancellationToken);
                if (users == null || users.Count == 0)
                {
                    throw new Exception("Users not found.");
                }

                var entities = await _moneyUsersUsedRepository.GetMoneyUsersUsedByRangeSubUsersIdAsync(users, cancellationToken);

                if (entities == null || entities.Count == 0)
                {
                    throw new Exception("MoneyUsersUsed records not found.");
                }

                var responseList = entities.Select(entity => new MoneyUsersUsedResponse
                {
                    Id = entity.Id.ToString(),
                    AmountMoney = entity.AmountMoney,
                    SubUserId = entity.SubUserId,
                }).ToList();

                return responseList;
            }
        }
    }
}
