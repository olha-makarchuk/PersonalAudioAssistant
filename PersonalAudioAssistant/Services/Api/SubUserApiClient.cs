﻿using PersonalAudioAssistant.Contracts.SubUser;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PersonalAudioAssistant.Services.Api
{
    namespace PersonalAudioAssistant.Services.Api
    {
        public class SubUserApiClient : BaseApiClient
        {
            public SubUserApiClient(HttpClient httpClient) : base(httpClient) { }

            public async Task<string> AddSubUserAsync(AddSubUserCommand command)
            {
                var url = $"{BaseUrl}SubUser/create";

                using var form = new MultipartFormDataContent();

                // Додаємо файл
                using var fileStream = File.OpenRead(command.PhotoPath);
                var fileName = Path.GetFileName(command.PhotoPath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                form.Add(fileContent, "file", fileName); // ключ — "file", бо на сервері FromForm IFormFile file

                // Створюємо новий об'єкт без шляху до фото, замінивши його на null
                var commandToSend = new
                {
                    command.UserName,
                    command.StartPhrase,
                    command.EndPhrase,
                    command.EndTime,
                    command.VoiceId,
                    command.UserVoice,
                    command.Password,
                    command.UserId,
                    Photo = (string)null // просто заглушка, бо файл уже окремо
                };

                // Серіалізуємо команду як JSON
                var json = JsonSerializer.Serialize(commandToSend);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                form.Add(jsonContent, "command"); // ключ має збігатися з ім’ям параметра в методі API, якщо він був би позначений як [FromForm] AddSubUserCommand command

                var response = await _httpClient.PostAsync(url, form);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
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
                var url = $"{BaseUrl}SubUser/change-photo";

                using var form = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(photoPath);
                var fileName = Path.GetFileName(photoPath);

                // Додаємо файл з правильним ключем — має збігатися з назвою властивості у класі
                form.Add(new StreamContent(fileStream), "Photo", fileName);

                // Додаємо текстове поле
                form.Add(new StringContent(photoURL), "PhotoURL");

                var response = await _httpClient.PostAsync(url, form);
                response.EnsureSuccessStatusCode();
            }

            public async Task UpdateSubUserAsync(UpdateSubUserCommand subUser)
            {
                var url = $"{BaseUrl}SubUser";

                var payload = new
                {
                    Id = subUser.Id,
                    UserId = subUser.UserId ?? string.Empty,
                    UserName = subUser.UserName ?? string.Empty,
                    StartPhrase = subUser.StartPhrase ?? string.Empty,
                    EndPhrase = subUser.EndPhrase ?? string.Empty,
                    EndTime = subUser.EndTime ?? string.Empty,
                    VoiceId = subUser.VoiceId ?? string.Empty,
                    Password = subUser.Password ?? string.Empty,
                    NewPassword = subUser.NewPassword ?? string.Empty,
                    PhotoPath = subUser.PhotoPath ?? string.Empty
                };

                await PutAsync<object, string>(url, payload);
            }

            public async Task UpdatePersonalInformationAsync(UpdatePersonalInformationCommand subUser)
            {
                var url = $"{BaseUrl}SubUser/update-personal-information";

                var payload = new
                {
                    Id = subUser.Id,
                    UserId = subUser.UserId ?? string.Empty,
                    UserName = subUser.UserName ?? string.Empty,
                    StartPhrase = subUser.StartPhrase ?? string.Empty,
                    EndPhrase = subUser.EndPhrase ?? string.Empty,
                    EndTime = subUser.EndTime ?? string.Empty,
                };

                await PostAsync<object, string>(url, payload);
            }

            public async Task UpdatePasswordAsync(UpdatePasswordCommand subUser)
            {
                var url = $"{BaseUrl}SubUser/update-password";

                var payload = new
                {
                    Id = subUser.Id,
                    Password = subUser.Password,
                    NewPassword = subUser.NewPassword
                };

                await PostAsync<object, string>(url, payload);
            }

            public async Task UpdateUserVoiceAsync(string userId, byte[] userVoice)
            {
                var url = $"{BaseUrl}SubUser/update-user-voice";

                var payload = new
                {
                    UserId = userId,
                    UserVoice = userVoice,
                };

                await PostAsync<object, string>(url, payload);
            }

            public async Task UpdateVoiceAsync(string id, string voiceId)
            {
                var url = $"{BaseUrl}SubUser/update-voice";

                var payload = new
                {
                    Id = id,
                    VoiceId = voiceId
                };

                await PostAsync<object, string>(url, payload);
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
            public byte[] UserVoice { get; set; }
            public string? Password { get; set; }
            public string? NewPassword { get; set; }
            public string PhotoPath { get; set; }
        }

        public class UpdatePersonalInformationCommand
        {
            public string? Id { get; set; }
            public string? UserId { get; set; }
            public string? UserName { get; set; }
            public string? StartPhrase { get; set; }
            public string? EndPhrase { get; set; }
            public string? EndTime { get; set; }
        }

        public class UpdatePasswordCommand
        {
            public string? Id { get; set; }
            public string? Password { get; set; }
            public string? NewPassword { get; set; }
        }
    }
}
