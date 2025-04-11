using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery
{
    public class GetSettingsByUserIdQuery : IRequest<AppSettings>
    {
        public string? UserId { get; set; }

        public class GetSettingsByUserIdQueryHandler : IRequestHandler<GetSettingsByUserIdQuery, AppSettings>
        {
            private readonly IAppSettingsRepository _appSettingsRepository;
            public GetSettingsByUserIdQueryHandler(IAppSettingsRepository appSettingsRepository)
            {
                _appSettingsRepository = appSettingsRepository;
            }
            public async Task<AppSettings> Handle(GetSettingsByUserIdQuery query, CancellationToken cancellationToken)
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
