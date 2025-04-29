using MediatR;
using PersonalAudioAssistant.Contracts.MainUser;

namespace PersonalAudioAssistant.Services.Api
{
    public class MainUserApiClient : BaseApiClient
    {
        public MainUserApiClient(HttpClient httpClient) : base(httpClient) { }

        public async Task<MainUserResponse> ChangePasswordAsync(string email, string password, string newPassword)
        {
            var url = $"{BaseUrl}MainUser";

            var request = new
            {
                Email = email,
                Password = password,
                NewPassword = newPassword
            };

            var mainUser = await PostAsync<object, MainUserResponse>(url, request);

            return mainUser;
        }

        public async Task<MainUserResponse> GetMainUserByEmailAsync(string email)
        {
            var url = $"{BaseUrl}MainUser/byemail";

            var request = new
            {
                Email = email
            };

            var mainUser = await PostAsync<object, MainUserResponse>(url, request);

            return mainUser;
        }

        public async Task UpdateMainUserAsync(MainUserResponse mainUser)
        {
            var url = $"{BaseUrl}MainUser/update-mainuser";

            var request = new
            {
                Id = mainUser.id,
                Email = mainUser.email,
                PasswordHash = mainUser.passwordHash,
                PasswordSalt = mainUser.passwordSalt,
                RefreshToken = mainUser.refreshToken,
                RefreshTokenExpiryTime = mainUser.refreshTokenExpiryTime
            };

            await PostAsync<object, string>(url, request);
        }

        public async Task CreateMainUserAsync(MainUser mainUser)
        {
            var url = $"{BaseUrl}MainUser/create";

            var request = new
            {
                Id = mainUser.Id,
                Email = mainUser.Email,
                PasswordHash = mainUser.PasswordHash,
                PasswordSalt = mainUser.PasswordSalt,
                RefreshToken = mainUser.RefreshToken,
                RefreshTokenExpiryTime = mainUser.RefreshTokenExpiryTime
            };

            await PostAsync<object, string>(url, request);
        }
    }

    public class UpdateMainUserCommand
    {
        public required string? Id { get; set; }
        public string? Email { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
