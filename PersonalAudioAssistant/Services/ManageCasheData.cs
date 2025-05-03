using MediatR;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Contracts.AppSettings;
using PersonalAudioAssistant.Services.Api;
using PersonalAudioAssistant.Services.Api.PersonalAudioAssistant.Services.Api;

namespace PersonalAudioAssistant.Services
{
    public class ManageCacheData
    {
        private const string UsersCacheKey = "UsersList";
        private const string SettingsCacheKey = "AppSettings";
        private const string ConversationCacheKey = "Conversation";

        private readonly IMemoryCache _cache;
        private readonly IMediator _mediator;
        private readonly AppSettingsApiClient _appSettingsApiClient;
        private readonly SubUserApiClient _subUserApiClient;
        private readonly ConversationApiClient _conversationApiClient;
        private readonly VoiceApiClient _voiceApiClient;

        public ManageCacheData(IMemoryCache cache, IMediator mediator, AppSettingsApiClient appSettingsApiClient, SubUserApiClient subUserApiClient, ConversationApiClient conversationApiClient, VoiceApiClient voiceApiClient)
        {
            _cache = cache;
            _mediator = mediator;
            _appSettingsApiClient = appSettingsApiClient;
            _subUserApiClient = subUserApiClient;
            _conversationApiClient = conversationApiClient;
            _voiceApiClient = voiceApiClient;
        }

        public async Task<List<SubUserResponse>> GetUsersAsync(Action<double> progress = null)
        {
            if (!_cache.TryGetValue(UsersCacheKey, out List<SubUserResponse> users))
            {
                await Task.Delay(1000);
                users = await _subUserApiClient.GetAllUsersByUserIdAsync(await SecureStorage.GetAsync("user_id"));
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

        public async Task<ConversationIdResponse> GetСonversationAsync(Action<double> progress = null)
        {
            if (!_cache.TryGetValue(ConversationCacheKey, out ConversationIdResponse conversation))
            {
                await Task.Delay(800);
                var userId = await SecureStorage.GetAsync("user_id");
                var conversations = await _conversationApiClient.GetConversationsBySubUserIdAsync(userId, 1, 1);

                conversation = new ConversationIdResponse
                {
                    ConversationId = conversations[0].IdConversation
                };

                progress?.Invoke(0.7);
                await Task.Delay(700);
                _cache.Set(SettingsCacheKey, conversation);
                progress?.Invoke(1.0);
            }
            return conversation;
        }

        public async Task<AppSettingsResponse> GetAppSettingsAsync(Action<double> progress = null)
        {
            if (!_cache.TryGetValue(SettingsCacheKey, out AppSettingsResponse settings))
            {
                await Task.Delay(800); 
                var userId = await SecureStorage.GetAsync("user_id");
                settings = await _appSettingsApiClient.GetSettingsByUserIdAsync(userId);
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
            await GetAppSettingsAsync();
        }

        public void ClearCache()
        {
            _cache.Remove(UsersCacheKey);
            _cache.Remove(SettingsCacheKey);
            _cache.Remove(ConversationCacheKey);
        }
    }
}