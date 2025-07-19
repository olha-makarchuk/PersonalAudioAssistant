using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface IPaymentHistoryRepository
    {
        Task<List<PaymentHistory>> GetPaymentsHistoryByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task UpdatePaymentHistoryAsync(PaymentHistory paymentHistory, CancellationToken cancellationToken);
        Task AddPaymentHistoryAsync(PaymentHistory paymentHistory, CancellationToken cancellationToken);
    }
}
