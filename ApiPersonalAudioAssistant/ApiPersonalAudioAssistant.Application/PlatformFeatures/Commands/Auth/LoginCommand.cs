using MediatR;
using Microsoft.Extensions.Configuration;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.Auth;
using System.Security.Claims;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth
{
    public class LoginCommand : IRequest<MainUserLoginResponse>
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, MainUserLoginResponse>
    {
        private readonly IMainUserRepository _mainUserRepository;
        private readonly TokenBase _tokenApiBase;
        private readonly IConfiguration _configuration;
        private readonly PasswordManager _passwordManager;

        public LoginCommandHandler(IMainUserRepository mainUserRepository, TokenBase tokenApiBase, IConfiguration configuration, PasswordManager passwordManager)
        {
            _mainUserRepository = mainUserRepository;
            _tokenApiBase = tokenApiBase;
            _configuration = configuration;
            _passwordManager = passwordManager;
        }

        public async Task<MainUserLoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]!);
            var TokenExpiryTimeInHours = double.Parse(_configuration["JWTKey:ExpiryHours"]!);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Заповніть всі поля");

            var userExist = await _mainUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            if (userExist == null)
            {
                throw new ApplicationException($"Користувача з таким email не зареєстровано");
            }

            if (!_passwordManager.VerifyPasswordHash(request.Password, userExist.PasswordHash, userExist.PasswordSalt))
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
                Id = userExist.Id.ToString(),
                UserId = userExist.Id.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessExpiresAt = DateTime.UtcNow.AddHours(TokenExpiryTimeInHours),
                RefreshExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
            };
        }
    }
}
