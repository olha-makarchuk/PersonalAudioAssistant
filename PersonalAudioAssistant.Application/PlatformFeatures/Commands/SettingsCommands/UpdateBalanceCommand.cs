using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands
{
    public class UpdateBalanceCommand: IRequest
    {
        public string? UserId { get; set; }
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
