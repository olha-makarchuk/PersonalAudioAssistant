using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.Auth;
using System.Security.Claims;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth
{
    public class RefreshTokenCommand : IRequest<TokenApiResponse>
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenApiResponse>
    {
        private readonly IMainUserRepository _mainUserRepository;
        private readonly TokenBase _tokenBase;
        private readonly IConfiguration _configuration;

        public RefreshTokenCommandHandler(IMainUserRepository mainUserRepository, TokenBase tokenBase, IConfiguration configuration)
        {
            _mainUserRepository = mainUserRepository;
            _tokenBase = tokenBase;
            _configuration = configuration;
        }

        public async Task<TokenApiResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken = default)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]!);

            var principal = _tokenBase.GetPrincipalFromExpiredToken(request.AccessToken);

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Invalid token: no email found");
            }

            var user = await _mainUserRepository.GetUserByEmailAsync(email, cancellationToken);
            if (user is null)
            {
                throw new Exception("User not found");
            }

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Invalid refresh token");
            }

            var newAccessToken = _tokenBase.GenerateToken(principal.Claims);
            var newRefreshToken = TokenBase.GenerateRefreshToken();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = newRefreshTokenExpiry;
            await _mainUserRepository.UpdateUser(user, cancellationToken);

            return new TokenApiResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = newRefreshTokenExpiry
            };
        }
    }
}