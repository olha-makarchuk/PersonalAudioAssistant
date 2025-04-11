﻿using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> GetPaymentByIdAsync(string Id, CancellationToken cancellationToken);
        Task<Payment> GetPaymentByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken);
        Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken);
    }
}
