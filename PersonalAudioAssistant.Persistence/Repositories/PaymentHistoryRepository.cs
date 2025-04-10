using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class PaymentHistoryRepository : IPaymentHistoryRepository
    {
        public Task AddPaymentHistoryAsync(PaymentHistory paymentHistory)
        {
            throw new NotImplementedException();
        }

        public Task DeletePaymentHistoryAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PaymentHistory> GetPaymentHistoryByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PaymentHistory> GetPaymentHistoryByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePaymentHistoryAsync(PaymentHistory paymentHistory)
        {
            throw new NotImplementedException();
        }
    }
}
