using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.AppSettings;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery
{
    public class GetSettingsByUserIdQuery : IRequest<AppSettingsResponse>
    {
        public required string UserId { get; set; }

        public class GetSettingsByUserIdQueryHandler : IRequestHandler<GetSettingsByUserIdQuery, AppSettingsResponse>
        {
            private readonly IAppSettingsRepository _appSettingsRepository;
            public GetSettingsByUserIdQueryHandler(IAppSettingsRepository appSettingsRepository)
            {
                _appSettingsRepository = appSettingsRepository;
            }
            public async Task<AppSettingsResponse> Handle(GetSettingsByUserIdQuery query, CancellationToken cancellationToken)
            {
                var user = await _appSettingsRepository.GetSettingsByUserIdAsync(query.UserId, cancellationToken);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var userResponse = new AppSettingsResponse
                {
                    Id = user.Id.ToString(),
                    Balance = user.Balance,
                    Theme = user.Theme,
                    UserId = user.UserId,
                };

                return userResponse;
            }
        }
    }
}
