using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.MainUserQuery;
using PersonalAudioAssistant.Domain.Entities;
using System.ComponentModel;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery
{
    public class GetSettingsByEmailQuery : IRequest<AppSettings>
    {
        public string? UserId { get; set; }

        public class GetSettingsByEmailQueryHandler : IRequestHandler<GetSettingsByEmailQuery, AppSettings>
        {
            private readonly IAppSettingsRepository _appSettingsRepository;
            public GetSettingsByEmailQueryHandler(IAppSettingsRepository appSettingsRepository)
            {
                _appSettingsRepository = appSettingsRepository;
            }
            public async Task<AppSettings> Handle(GetSettingsByEmailQuery query, CancellationToken cancellationToken)
            {
                var user = await _appSettingsRepository.GetSettingsByUserIdAsync(query.UserId, cancellationToken);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                return user;
            }
        }
    }
}
