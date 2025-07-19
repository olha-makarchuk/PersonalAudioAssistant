using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface IMoneyUsedRepository
    {
        Task<List<MoneyUsed>> GetMoneyUsedByMainUserIdAsync(string mainUserId, CancellationToken cancellationToken);
        Task AddMoneyUsedAsync(MoneyUsed moneyUsed, CancellationToken cancellationToken);
        Task UpdateMoneyUsedAsync(MoneyUsed moneyUsed, CancellationToken cancellationToken);
        Task<MoneyUsed> GetMoneyUsedbyMainIdAndDateAsync(string mainUsedId, DateTime dateTime, CancellationToken cancellationToken);
    }
}
