using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth
{
    public class LoginCommand : IRequest<MainUserLoginResponse>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, MainUserLoginResponse>
    {
        private readonly IMainUserRepository _mainUserRepository;
        private readonly TokenBase _tokenApiBase;
        private readonly IConfiguration _configuration;

        public LoginCommandHandler(IMainUserRepository mainUserRepository, TokenBase tokenApiBase, IConfiguration configuration)
        {
            _mainUserRepository = mainUserRepository;
            _tokenApiBase = tokenApiBase;
            _configuration = configuration;
        }

        public async Task<MainUserLoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]);
            var TokenExpiryTimeInHours = double.Parse(_configuration["JWTKey:ExpiryHours"]);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Заповніть всі поля");

            var userExist = await _mainUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            if (userExist == null)
            {
                throw new ApplicationException($"Користувача з таким email не зареєстровано");
            }

            if (!VerifyPasswordHash(request.Password, userExist.PasswordHash, userExist.PasswordSalt))
            {
                throw new Exception("Логін або пароль не вірні");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userExist.Email),
                new Claim(ClaimTypes.NameIdentifier, userExist.Id.ToString())
            };

            string accessToken = _tokenApiBase.GenerateToken(claims);
            string refreshToken = TokenBase.GenerateRefreshToken();

            userExist.RefreshToken = refreshToken;
            userExist.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
            await _mainUserRepository.UpdateUser(userExist, cancellationToken);

            return new MainUserLoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessExpiresAt = DateTime.UtcNow.AddHours(TokenExpiryTimeInHours),
                RefreshExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
            };
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }

    public class MainUserLoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessExpiresAt { get; set; }
        public DateTime RefreshExpiresAt { get; set; }
    }

}
