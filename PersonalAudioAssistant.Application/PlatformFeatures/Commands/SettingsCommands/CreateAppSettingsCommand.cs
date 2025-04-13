using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class CreateAppSettingsCommand: IRequest
    {
        public string? UserId { get; set; }
    }

    public class CreateAppSettingsCommandHandler : IRequestHandler<CreateAppSettingsCommand>
    {
        private readonly IAppSettingsRepository _appSettingsRepository;

        public CreateAppSettingsCommandHandler(IAppSettingsRepository appSettingsRepository)
        {
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task Handle(CreateAppSettingsCommand request, CancellationToken cancellationToken = default)
        {
            var appSettings = new AppSettings()
            {
                UserId = request.UserId,
                Balance = 0,
                Theme = "Light"
            };

            await _appSettingsRepository.AddSettingsAsync(appSettings, cancellationToken);
        }
    }
}
