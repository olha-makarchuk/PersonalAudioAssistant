using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        public Task AddPaymentAsync(Payment payment)
        {
            throw new NotImplementedException();
        }

        public Task DeletePaymentAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Payment> GetPaymentByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Payment> GetPaymentByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePaymentAsync(Payment payment)
        {
            throw new NotImplementedException();
        }
    }
}
