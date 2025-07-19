using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MoneyUsedCommands
{
    public class CreateMoneyUsedCommand : IRequest<Unit>
    {
        public string SubUserId { get; set; }
        public string MainUserId { get; set; }
        public decimal AmountMoney { get; set; }
    }

    public class CreateMoneyUsersUsedCommandHandler : IRequestHandler<CreateMoneyUsedCommand, Unit>
    {
        private readonly IMoneyUsedRepository _moneyUsedRepository;
        private readonly IBlobStorage _blobStorage;

        public CreateMoneyUsersUsedCommandHandler(IMoneyUsedRepository moneyUsedRepository, IBlobStorage blobStorage)
        {
            _moneyUsedRepository = moneyUsedRepository;
            _blobStorage = blobStorage;
        }

        public async Task<Unit> Handle(CreateMoneyUsedCommand request, CancellationToken cancellationToken = default)
        {
            var entityExists = await _moneyUsedRepository.GetMoneyUsedbyMainIdAndDateAsync(request.MainUserId, DateTime.UtcNow, cancellationToken);

            if (entityExists == null)
            {
                var entity = new MoneyUsed()
                {
                    DateTimeUsed = DateTime.UtcNow,
                    AmountMoney = request.AmountMoney,
                    MainUserId = request.MainUserId
                };

                await _moneyUsedRepository.AddMoneyUsedAsync(entity, cancellationToken);
            }
            else
            {
                entityExists.AmountMoney += request.AmountMoney;
                await _moneyUsedRepository.UpdateMoneyUsedAsync(entityExists, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
