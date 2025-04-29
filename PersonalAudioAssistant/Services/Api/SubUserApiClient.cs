using MediatR;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Contracts.Voice;

namespace PersonalAudioAssistant.Services.Api
{
    namespace PersonalAudioAssistant.Services.Api
    {
        public class SubUserApiClient : BaseApiClient
        {
            public SubUserApiClient(HttpClient httpClient) : base(httpClient) { }

            public async Task<string> AddSubUserAsync(AddSubUserCommand command)
            {
                var url = $"{BaseUrl}SubUser";

                // Упевненість, що поля не null
                var payload = new
                {
                    UserName = command.UserName,
                    StartPhrase = command.StartPhrase,
                    EndPhrase = command.EndPhrase ?? string.Empty,
                    EndTime = command.EndTime ?? string.Empty,
                    VoiceId = command.VoiceId,
                    UserVoice = command.UserVoice,
                    PhotoPath = command.PhotoPath ?? string.Empty,
                    UserId = command.UserId
                };

                return await PostAsync<object, string>(url, payload);
            }

            public async Task DeletePasswordSubUserAsync(string userId, string password)
            {
                var url = $"{BaseUrl}SubUser/password";

                var payload = new
                {
                    UserId = userId,
                    Password = password
                };

                await DeleteAsync(url, payload);
            }

            public async Task DeleteSubUserAsync(string userId)
            {
                var url = $"{BaseUrl}SubUser";

                var payload = new
                {
                    UserId = userId
                };

                await DeleteAsync(url, payload);
            }

            public async Task UpdatePhotoAsync(string photoURL, string photoPath)
            {
                var url = $"{BaseUrl}SubUser/photo";

                var payload = new
                {
                    PhotoURL = photoURL,
                    PhotoPath = photoPath
                };

                await PutAsync<object, string>(url, payload);
            }

            public async Task UpdateSubUserAsync(UpdateSubUserCommand subUser)
            {
                var url = $"{BaseUrl}SubUser";

                var payload = new
                {
                    Id = subUser.Id,
                    UserId = subUser.UserId,
                    UserName = subUser.UserName,
                    StartPhrase = subUser.StartPhrase,
                    EndPhrase = subUser.EndPhrase ?? string.Empty,
                    EndTime = subUser.EndTime ?? string.Empty,
                    VoiceId = subUser.VoiceId,
                    UserVoice = subUser.UserVoice,
                    Password = subUser.Password ?? string.Empty,
                    NewPassword = subUser.NewPassword ?? string.Empty,
                    PhotoPath = subUser.PhotoPath ?? string.Empty
                };

                await PutAsync<object, string>(url, payload);
            }

            public async Task<bool> CheckSubUserPasswordAsync(string userId, string password)
            {
                var url = $"{BaseUrl}SubUser/check-password";

                var payload = new
                {
                    UserId = userId,
                    Password = password
                };

                return await PostAsync<object, bool>(url, payload);
            }

            public async Task<List<SubUserResponse>> GetAllUsersByUserIdAsync(string userId)
            {
                var url = $"{BaseUrl}SubUser/allusers";

                var payload = new
                {
                    UserId = userId,
                };

                return await PostAsync<object, List<SubUserResponse>>(url, payload);
            }

            public async Task<SubUserResponse> GetUserByIdAsync(string userId)
            {
                var url = $"{BaseUrl}SubUser/userbyid";

                var payload = new
                {
                    UserId = userId,
                };

                return await PostAsync<object, SubUserResponse>(url, payload);
            }

            public async Task<SubUserResponse> GetUserByStartPhraseAsync(string startPhrase)
            {
                var url = $"{BaseUrl}SubUser/userbystartphrase";

                var payload = new
                {
                    StartPhrase = startPhrase,
                };

                return await PostAsync<object, SubUserResponse>(url, payload);
            }
        }

        public class AddSubUserCommand
        {
            public required string UserName { get; set; }
            public required string StartPhrase { get; set; }
            public string? EndPhrase { get; set; }
            public string? EndTime { get; set; }
            public required string VoiceId { get; set; }
            public required List<double> UserVoice { get; set; }
            public string? Password { get; set; }
            public required string UserId { get; set; }
            public required string PhotoPath { get; set; }
        }

        public class UpdateSubUserCommand
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string StartPhrase { get; set; }
            public string? EndPhrase { get; set; }
            public string? EndTime { get; set; }
            public string VoiceId { get; set; }
            public Stream UserVoice { get; set; }
            public string? Password { get; set; }
            public string? NewPassword { get; set; }
            public string PhotoPath { get; set; }
        }
    }
}
