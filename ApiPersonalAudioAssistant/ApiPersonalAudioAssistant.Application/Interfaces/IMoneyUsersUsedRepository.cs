using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface IMoneyUsersUsedRepository
    {
        Task<List<MoneyUsersUsed>> GetMoneyUsersUsedByRangeSubUsersIdAsync(List<SubUser> subUsersId, CancellationToken cancellationToken);
        Task AddMoneyUsersUsedAsync(MoneyUsersUsed moneyUsersUsed, CancellationToken cancellationToken);
        Task UpdateMoneyUsersUsedAsync(MoneyUsersUsed moneyUsersUsed, CancellationToken cancellationToken);
        Task<MoneyUsersUsed> GetMoneyUsersUsedbySubUserIdAsync(string subUserId, CancellationToken cancellationToken);
    }
}
