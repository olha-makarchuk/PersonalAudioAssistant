using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class UpdateBalanceCommand: IRequest
    {
        public required string UserId { get; set; }
        public int Balance { get; set; }
    }

    public class UpdateBalanceCommandHandler : IRequestHandler<UpdateBalanceCommand>
    {
        private readonly IAppSettingsRepository _settingsRepository;

        public UpdateBalanceCommandHandler(IAppSettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task Handle(UpdateBalanceCommand request, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsRepository.GetSettingsByUserIdAsync(request.UserId, cancellationToken);

            if (settings == null)
            {
                throw new Exception("Settings not found.");
            }

            settings.Balance = request.Balance;
            await _settingsRepository.UpdateSettingsAsync(settings, cancellationToken);
        }
    }
}
