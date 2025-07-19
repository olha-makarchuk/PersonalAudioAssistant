using Microsoft.EntityFrameworkCore;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;
using System.Threading;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
{
    public class PaymentHistoryRepository : IPaymentHistoryRepository
    {
        private readonly CosmosDbContext _context;
        public PaymentHistoryRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task AddPaymentHistoryAsync(PaymentHistory paymentHistory, CancellationToken cancellationToken)
        {
            await _context.PaymentHistory.AddAsync(paymentHistory, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<PaymentHistory>> GetPaymentsHistoryByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            var payment = await _context.PaymentHistory
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return payment;
        }

        public async Task UpdatePaymentHistoryAsync(PaymentHistory paymentHistory, CancellationToken cancellationToken)
        {
            _context.PaymentHistory.Update(paymentHistory);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
