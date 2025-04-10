using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class AutoPaymentRepository : IAutoPaymentRepository
    {
        public Task AddAutoPaymentAsync(AutoPayments autoPayment)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAutoPaymentAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<AutoPayments> GetAutoPaymentByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<AutoPayments> GetAutoPaymentByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAutoPaymentAsync(AutoPayments autoPayment)
        {
            throw new NotImplementedException();
        }
    }
}
