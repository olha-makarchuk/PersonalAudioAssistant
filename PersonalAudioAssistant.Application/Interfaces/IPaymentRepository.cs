using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> GetPaymentByIdAsync(string userId);
        Task<Payment> GetPaymentByUserIdAsync(string userId);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(string userId);
        Task AddPaymentAsync(Payment payment);
    }
}
