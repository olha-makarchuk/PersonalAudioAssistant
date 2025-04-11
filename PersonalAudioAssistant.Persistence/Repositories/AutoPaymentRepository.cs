using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Persistence.Context;

namespace PersonalAudioAssistant.Persistence.Repositories
{
    public class AutoPaymentRepository : IAutoPaymentRepository
    {
        private readonly CosmosDbContext _context;
        public AutoPaymentRepository(CosmosDbContext context)
        {
            _context = context;
        }

        public async Task AddAutoPaymentAsync(AutoPayments autoPayment, CancellationToken cancellationToken)
        {
            await _context.AutoPayments.AddAsync(autoPayment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<AutoPayments> GetAutoPaymentByIdAsync(string id, CancellationToken cancellationToken)
        {
            Guid guidId = Guid.Parse(id);
            return await _context.AutoPayments.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
        }

        public async Task<AutoPayments> GetAutoPaymentByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _context.AutoPayments.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task UpdateAutoPaymentAsync(AutoPayments autoPayment, CancellationToken cancellationToken)
        {
            _context.AutoPayments.Update(autoPayment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
