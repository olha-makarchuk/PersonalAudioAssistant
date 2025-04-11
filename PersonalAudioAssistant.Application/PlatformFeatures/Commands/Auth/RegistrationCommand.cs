using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth
{
    public class RegistrationCommand : IRequest<MainUserRegisterResponse>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class RegistrationCommandHandler : IRequestHandler<RegistrationCommand, MainUserRegisterResponse>
    {
        private readonly IMainUserRepository _mainUserRepository;
        private readonly TokenBase _tokenApiBase;
        private readonly IConfiguration _configuration;
        private readonly IAppSettingsRepository _appSettingsRepository;

        public RegistrationCommandHandler(IMainUserRepository mainUserRepository, TokenBase tokenApiBase, IConfiguration configuration, IAppSettingsRepository appSettingsRepository)
        {
            _mainUserRepository = mainUserRepository;
            _tokenApiBase = tokenApiBase;
            _configuration = configuration;
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task<MainUserRegisterResponse> Handle(RegistrationCommand request, CancellationToken cancellationToken = default)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]);
            var TokenExpiryTimeInHours = double.Parse(_configuration["JWTKey:ExpiryHours"]);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Заповніть всі поля");

            var existingUser = await _mainUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
                throw new InvalidOperationException("Користувач з таким email вже існує");

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new MainUser
            {
                Email = request.Email,
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

            await _mainUserRepository.CreateUser(user, cancellationToken);
            
            var userNew = await _mainUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);

            return new MainUserRegisterResponse
            {
                UserId = userNew.Id.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessExpiresAt = DateTime.UtcNow.AddHours(TokenExpiryTimeInHours),
                RefreshExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
            };
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public class MainUserRegisterResponse
    {
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessExpiresAt { get; set; }
        public DateTime RefreshExpiresAt { get; set; }
    }
}