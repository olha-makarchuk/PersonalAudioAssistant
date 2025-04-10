using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IPaymentHistoryRepository
    {
        Task<PaymentHistory> GetPaymentHistoryByIdAsync(string userId);
        Task<PaymentHistory> GetPaymentHistoryByUserIdAsync(string userId);
        Task UpdatePaymentHistoryAsync(PaymentHistory paymentHistory);
        Task DeletePaymentHistoryAsync(string userId);
        Task AddPaymentHistoryAsync(PaymentHistory paymentHistory);
    }
}
