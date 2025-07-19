using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class CreateAppSettingsCommand : IRequest<Unit>
    {
        public string? UserId { get; set; }
    }

    public class CreateAppSettingsCommandHandler : IRequestHandler<CreateAppSettingsCommand, Unit>
    {
        private readonly IAppSettingsRepository _appSettingsRepository;

        public CreateAppSettingsCommandHandler(IAppSettingsRepository appSettingsRepository)
        {
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task<Unit> Handle(CreateAppSettingsCommand request, CancellationToken cancellationToken)
        {
            var appSettings = new AppSettings
            {
                UserId = request.UserId,
                Balance = 0,
                Theme = "Light"
            };

            await _appSettingsRepository.AddSettingsAsync(appSettings, cancellationToken);
            return Unit.Value;
        }
    }
}
