using Microsoft.EntityFrameworkCore;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using ApiPersonalAudioAssistant.Persistence.Context;

namespace ApiPersonalAudioAssistant.Persistence.Repositories
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
            var autoPayment = await _context.AutoPayments.FirstOrDefaultAsync(x => x.Id == guidId, cancellationToken);
            return autoPayment!;
        }

        public async Task<AutoPayments> GetAutoPaymentByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            var autoPayment = await _context.AutoPayments.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            return autoPayment!;
        }

        public async Task UpdateAutoPaymentAsync(AutoPayments autoPayment, CancellationToken cancellationToken)
        {
            _context.AutoPayments.Update(autoPayment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
