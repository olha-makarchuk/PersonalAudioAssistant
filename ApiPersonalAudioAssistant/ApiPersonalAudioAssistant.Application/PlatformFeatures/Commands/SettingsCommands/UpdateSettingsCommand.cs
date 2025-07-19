using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class UpdateSettingsCommand : IRequest<Unit>
    {
        public string? UserId { get; set; }
        public decimal? Balance { get; set; }
        public string? Theme { get; set; }
    }

    public class UpdateSettingsCommandHandler : IRequestHandler<UpdateSettingsCommand, Unit>
    {
        private readonly IAppSettingsRepository _appSettingsRepository;

        public UpdateSettingsCommandHandler(IAppSettingsRepository appSettingsRepository)
        {
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task<Unit> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
        {
            var existing = await _appSettingsRepository.GetSettingsByUserIdAsync(request.UserId, cancellationToken);
            if (existing == null)
            {
                // handle not found: throw or return
                throw new KeyNotFoundException("Settings not found for user: " + request.UserId);
            }

            // update fields
            if (request.Balance.HasValue)
                existing.Balance = request.Balance.Value;
            if (!string.IsNullOrEmpty(request.Theme))
                existing.Theme = request.Theme;

            await _appSettingsRepository.UpdateSettingsAsync(existing, cancellationToken);
            return Unit.Value;
        }
    }
}
