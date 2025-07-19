using ApiPersonalAudioAssistant.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly CosmosDbContext _context;
        public PaymentRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            await _context.Payment.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Payment> GetPaymentByIdAsync(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);
            var payment = await _context.Payment.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            return payment!;
        }

        public async Task<Payment> GetPaymentByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            var payment = await _context.Payment.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            return payment;
        }

        public async Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            _context.Payment.Update(payment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
