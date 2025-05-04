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

        public async Task<string?> GetConversationAsync(Action<double>? progress = null)
        {
            // 1. Спроба взяти з кешу
            if (_cache.TryGetValue(ConversationCacheKey, out ConversationIdResponse cached))
            {
                progress?.Invoke(1.0);
                return cached.ConversationId;
            }

            // 2. Якщо в кеші немає — починаємо «довгу» операцію
            progress?.Invoke(0.0);

            // а) невелика затримка, щоб показати прогрес-бар/анімоване очікування
            await Task.Delay(800);
            progress?.Invoke(0.2);

            // b) отримуємо ідентифікатор користувача
            var userId = await SecureStorage.GetAsync("user_id");
            progress?.Invoke(0.4);

            // c) запитуємо список бесід (першу сторінку, 1 елемент)
            var conversations = await _conversationApiClient
                .GetConversationsBySubUserIdAsync(userId, pageNumber: 1, pageSize: 1);
            progress?.Invoke(0.7);

            // 3. Якщо бесіди знайдено — кешуємо і повертаємо
            if (conversations != null && conversations.Any())
            {
                var response = new ConversationIdResponse
                {
                    ConversationId = conversations[0].IdConversation
                };
                _cache.Set(ConversationCacheKey, response);
                progress?.Invoke(1.0);
                return response.ConversationId;
            }

            // 4. Бесіди не знайдено — повертаємо null (або можна кидати виняток)
            progress?.Invoke(1.0);
            return null;
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