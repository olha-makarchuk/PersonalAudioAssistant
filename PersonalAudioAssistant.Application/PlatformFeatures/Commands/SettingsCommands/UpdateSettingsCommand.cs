using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class UpdateSettingsCommand : IRequest
    {
        public string? UserId { get; set; }
        public string? Theme { get; set; }
        public string? Payment { get; set; }
        public int MinTokenThreshold { get; set; }
        public int ChargeAmount { get; set; }
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
            /*
            var newSettings = new AppSettings()
            {
                Theme = request.Theme,
                Payment = request.Payment,
                MinTokenThreshold = request.MinTokenThreshold,
                ChargeAmount = request.ChargeAmount
            };

            await _appSettingsRepository.UpdateSettingsAsync(newSettings, cancellationToken);*/
        }
    }
}
