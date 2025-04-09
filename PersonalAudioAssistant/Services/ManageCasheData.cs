using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery;

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

        public async Task<List<SubUser>> GetUsersAsync()
        {
            if (!_cache.TryGetValue(UsersCacheKey, out List<SubUser> users))
            {
                users = await _mediator.Send(new GetAllUsersOuery());

                _cache.Set(UsersCacheKey, users);
            }

            return users;
        }

        public async Task<AppSettings> GetAppSettingsAsync()
        {
            if (!_cache.TryGetValue(SettingsCacheKey, out AppSettings settings))
            {
                var userId = await SecureStorage.GetAsync("user_id");
                settings = await _mediator.Send(new GetSettingsByUserIdQuery { UserId = userId });

                _cache.Set(SettingsCacheKey, settings);
            }

            return settings;
        }

        public void ClearCache()
        {
            _cache.Remove(UsersCacheKey);
            _cache.Remove(SettingsCacheKey);
        }
    }
}
