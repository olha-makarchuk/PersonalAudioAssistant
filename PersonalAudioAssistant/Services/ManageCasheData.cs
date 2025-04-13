using MediatR;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Contracts.AppSettings;

namespace PersonalAudioAssistant.Services
{
    public class ManageCacheData
    {
        private const string UsersCacheKey = "UsersList";
        private const string SettingsCacheKey = "AppSettings";

        private readonly IMemoryCache _cache;
        private readonly IMediator _mediator;

        public ManageCacheData(IMemoryCache cache, IMediator mediator)
        {
            _cache = cache;
            _mediator = mediator;
        }

        public async Task<List<SubUserResponse>> GetUsersAsync(Action<double> progress = null)
        {
            if (!_cache.TryGetValue(UsersCacheKey, out List<SubUserResponse> users))
            {
                await Task.Delay(1000); 
                users = await _mediator.Send(new GetAllUsersByUserIdQuery() { UserId = await SecureStorage.GetAsync("user_id")});
                progress?.Invoke(0.5); 
                await Task.Delay(1000);
                _cache.Set(UsersCacheKey, users);
                progress?.Invoke(1.0); 
            }

            return users;
        }

        public async Task UpdateUsersList()
        {
            _cache.Remove(UsersCacheKey);
            await GetUsersAsync();
        }

        public async Task<AppSettingsResponse> GetAppSettingsAsync(Action<double> progress = null)
        {
            if (!_cache.TryGetValue(SettingsCacheKey, out AppSettingsResponse settings))
            {
                await Task.Delay(800); 
                var userId = await SecureStorage.GetAsync("user_id");
                settings = await _mediator.Send(new GetSettingsByUserIdQuery { UserId = userId });
                progress?.Invoke(0.7);
                await Task.Delay(700);
                _cache.Set(SettingsCacheKey, settings);
                progress?.Invoke(1.0);
            }

            return settings;
        }

        public async Task UpdateAppSetttingsList()
        {
            _cache.Remove(SettingsCacheKey);
            await GetUsersAsync();
        }

        public void ClearCache()
        {
            _cache.Remove(UsersCacheKey);
            _cache.Remove(SettingsCacheKey);
        }
    }
}