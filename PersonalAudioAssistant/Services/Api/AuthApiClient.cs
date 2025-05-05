using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.Auth;
using System.Security.Claims;
using System.Threading;

namespace PersonalAudioAssistant.Services.Api
{
    public class AuthApiClient : BaseApiClient
    {
        private readonly TokenBase _tokenApiBase;
        private readonly IConfiguration _configuration;
        private readonly PasswordManager _passwordManager;
        private readonly MainUserApiClient _mainUserApiClient;

        public AuthApiClient(HttpClient httpClient, TokenBase tokenApiBase, IConfiguration configuration, PasswordManager passwordManager, MainUserApiClient mainUserApiClient) : base(httpClient)
        {
            _tokenApiBase = tokenApiBase;
            _configuration = configuration;
            _passwordManager = passwordManager;
            _mainUserApiClient = mainUserApiClient;
        }

        public async Task<MainUserLoginResponse> LoginAsync(string email, string password)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]!);
            var TokenExpiryTimeInHours = double.Parse(_configuration["JWTKey:ExpiryHours"]!);

            var userExist = await _mainUserApiClient.GetMainUserByEmailAsync(email); //

            if (userExist == null)
            {
                throw new ApplicationException($"Користувача з таким email не зареєстровано");
            }

            if (!_passwordManager.VerifyPasswordHash(password, userExist.passwordHash, userExist.passwordSalt))
            {
                throw new Exception("Логін або пароль не вірні");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userExist.email),
                new Claim(ClaimTypes.NameIdentifier, userExist.id.ToString())
            };

            string accessToken = _tokenApiBase.GenerateToken(claims);
            string refreshToken = TokenBase.GenerateRefreshToken();

            userExist.refreshToken = refreshToken;
            userExist.refreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

            await _mainUserApiClient.UpdateMainUserAsync(userExist);

            return new MainUserLoginResponse
            {
                Id = userExist.id.ToString(),
                UserId = userExist.id.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessExpiresAt = DateTime.UtcNow.AddHours(TokenExpiryTimeInHours),
                RefreshExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
            };
        }

        public async Task<string> LoginWithGoogleAsync(string email, string refreshToken, string password = null)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]!);

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Не всі передані поля заповнені", nameof(email));

            var user = await _mainUserApiClient.GetMainUserByEmailAsync(email);

            MainUser mainUser = new();

            if (user is null)
            {
                if (String.IsNullOrEmpty(password))
                {
                    _passwordManager.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
                    mainUser.PasswordHash = passwordHash;
                    mainUser.PasswordSalt = passwordSalt;
                }

                mainUser.Email = email;
                mainUser.RefreshToken = refreshToken;
                mainUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

                await _mainUserApiClient.CreateMainUserAsync(mainUser);
            }
            else
            {
                user.refreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
                user.refreshToken = refreshToken;

                await _mainUserApiClient.UpdateMainUserAsync(user);
            }

            return user!.id.ToString();
        }

        public async Task<TokenApiResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]!);

            var principal = _tokenApiBase.GetPrincipalFromExpiredToken(accessToken);

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Invalid token: no email found");
            }

            var user = await _mainUserApiClient.GetMainUserByEmailAsync(email);
            if (user is null)
            {
                throw new Exception("User not found");
            }

            if (user.refreshToken != refreshToken || user.refreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Invalid refresh token");
            }

            var newAccessToken = _tokenApiBase.GenerateToken(principal.Claims);
            var newRefreshToken = TokenBase.GenerateRefreshToken();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

            user.refreshToken = newRefreshToken;
            user.refreshTokenExpiryTime = newRefreshTokenExpiry;
            await _mainUserApiClient.UpdateMainUserAsync(user);

            return new TokenApiResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = newRefreshTokenExpiry
            };
        }

        public async Task<MainUserRegisterResponse> RegistrationAsync(string email, string password)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]!);
            var TokenExpiryTimeInHours = double.Parse(_configuration["JWTKey:ExpiryHours"]!);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Не всі поля заповнені");

            var existingUser = await _mainUserApiClient.GetMainUserByEmailAsync(email);
            if (existingUser != null)
                throw new InvalidOperationException("Користувач з таким email вже існує");

            _passwordManager.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new MainUser
            {
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            string accessToken = _tokenApiBase.GenerateToken(claims);
            string refreshToken = TokenBase.GenerateRefreshToken();
            user.RefreshToken = refreshToken;

            await _mainUserApiClient.CreateMainUserAsync(user);

            return new MainUserRegisterResponse
            {
                Id = user.Id.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessExpiresAt = DateTime.UtcNow.AddHours(TokenExpiryTimeInHours),
                RefreshExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
            };
        }
    }

    public class MainUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Email { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
