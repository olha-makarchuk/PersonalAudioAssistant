using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;

namespace PersonalAudioAssistant.Persistence.Repositories
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
            return await _context.Payment.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
        }

        public async Task<Payment> GetPaymentByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            var a = await _context.Payment.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            return a;
        }

        public async Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            _context.Payment.Update(payment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
