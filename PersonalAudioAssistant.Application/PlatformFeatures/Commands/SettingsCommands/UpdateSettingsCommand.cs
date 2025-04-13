using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class UpdateSettingsCommand : IRequest
    {
        public required string UserId { get; set; }
        public required string Theme { get; set; }
        public required int Balance { get; set; }
    }

    public class UpdateSettingsCommandHandler : IRequestHandler<UpdateSettingsCommand>
    {
        private readonly IAppSettingsRepository _appSettingsRepository;

        public UpdateSettingsCommandHandler(IAppSettingsRepository appSettingsRepository)
        {
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task Handle(UpdateSettingsCommand request, CancellationToken cancellationToken = default)
        {
            var settings = await _appSettingsRepository.GetSettingsByUserIdAsync(request.UserId, cancellationToken);
            if (settings == null)
            {
                throw new Exception("Settings not found");
            }
            
            var newSettings = new AppSettings()
            {
                Theme = request.Theme,
                Balance = request.Balance,
                UserId = request.UserId
            };

            await _appSettingsRepository.UpdateSettingsAsync(newSettings, cancellationToken);
        }
    }
}
