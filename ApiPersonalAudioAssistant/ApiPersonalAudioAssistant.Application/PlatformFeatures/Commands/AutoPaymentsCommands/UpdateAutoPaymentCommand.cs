using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands
{
    public class UpdateAutoPaymentCommand: IRequest<Unit>
    {
        public required string UserId { get; set; }
        public int MinTokenThreshold { get; set; }
        public int ChargeAmount { get; set; }
        public bool IsAutoPayment { get; set; }
    }

    public class UpdateAutoPaymentCommandHandler : IRequestHandler<UpdateAutoPaymentCommand, Unit>
    {
        private readonly IAutoPaymentRepository _autoPaymentsRepository;
        public UpdateAutoPaymentCommandHandler(IAutoPaymentRepository autoPaymentsRepository)
        {
            _autoPaymentsRepository = autoPaymentsRepository;
        }
        public async Task<Unit> Handle(UpdateAutoPaymentCommand request, CancellationToken cancellationToken)
        {
            var autoPayment = await _autoPaymentsRepository.GetAutoPaymentByUserIdAsync(request.UserId, cancellationToken);
            if (autoPayment == null)
            {
                throw new Exception("AutoPayment not found");
            }

            autoPayment.MinTokenThreshold = request.MinTokenThreshold;
            autoPayment.ChargeAmount = request.ChargeAmount;
            autoPayment.IsAutoPayment = request.IsAutoPayment;

            await _autoPaymentsRepository.UpdateAutoPaymentAsync(autoPayment, cancellationToken);
            return Unit.Value;
        }
    }
}
