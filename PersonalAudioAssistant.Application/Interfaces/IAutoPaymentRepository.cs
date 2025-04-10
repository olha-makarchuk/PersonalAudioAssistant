using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IAutoPaymentRepository
    {
        Task<AutoPayments> GetAutoPaymentByIdAsync(string userId);
        Task<AutoPayments> GetAutoPaymentByUserIdAsync(string userId);
        Task UpdateAutoPaymentAsync(AutoPayments autoPayment);
        Task DeleteAutoPaymentAsync(string userId);
        Task AddAutoPaymentAsync(AutoPayments autoPayment);
    }
}
