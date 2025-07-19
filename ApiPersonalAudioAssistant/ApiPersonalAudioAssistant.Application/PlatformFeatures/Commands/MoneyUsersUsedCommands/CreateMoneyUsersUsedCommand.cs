using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MoneyUsersUsedCommands
{
    public class CreateMoneyUsersUsedCommand : IRequest<Unit>
    {
        public string SubUserId { get; set; }
        public decimal AmountMoney { get; set; }
    }

    public class CreateMoneyUsersUsedCommandHandler : IRequestHandler<CreateMoneyUsersUsedCommand, Unit>
    {
        private readonly IMoneyUsersUsedRepository _moneyUsersUsedRepository;
        private readonly IBlobStorage _blobStorage;

        public CreateMoneyUsersUsedCommandHandler(IMoneyUsersUsedRepository moneyUsersUsedRepository, IBlobStorage blobStorage)
        {
            _moneyUsersUsedRepository = moneyUsersUsedRepository;
            _blobStorage = blobStorage;
        }

        public async Task<Unit> Handle(CreateMoneyUsersUsedCommand request, CancellationToken cancellationToken = default)
        {
            var entityExists = await _moneyUsersUsedRepository.GetMoneyUsersUsedbySubUserIdAsync(request.SubUserId, cancellationToken);
            if (entityExists == null)
            {
                var entity = new MoneyUsersUsed()
                {
                    AmountMoney = request.AmountMoney,
                    SubUserId = request.SubUserId
                };

                await _moneyUsersUsedRepository.AddMoneyUsersUsedAsync(entity, cancellationToken);
            }
            else
            {
                entityExists.AmountMoney += request.AmountMoney;
                await _moneyUsersUsedRepository.UpdateMoneyUsersUsedAsync(entityExists, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
