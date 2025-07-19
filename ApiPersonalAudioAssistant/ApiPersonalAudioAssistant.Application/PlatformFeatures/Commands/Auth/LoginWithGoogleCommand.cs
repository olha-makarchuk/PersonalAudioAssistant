using MediatR;
using Microsoft.Extensions.Configuration;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth
{
    public class LoginWithGoogleCommand : IRequest<string>
    {
        public required string Email { get; set; }
        public required string RefreshToken { get; set; }
    }

    public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, string>
    {
        private readonly IMainUserRepository _mainUserRepository;
        private readonly IConfiguration _configuration;
        private readonly IAppSettingsRepository _appSettingsRepository;

        public LoginWithGoogleCommandHandler(IMainUserRepository mainUserRepository, IConfiguration configuration, IAppSettingsRepository appSettingsRepository)
        {
            _mainUserRepository = mainUserRepository;
            _configuration = configuration;
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task<string> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken = default)
        {
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]!);

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Не всі передані поля заповнені", nameof(request.Email));

            var user = await _mainUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);

            MainUser mainUser = new();

            if (user is null)
            {
                mainUser.Email = request.Email;
                mainUser.RefreshToken = request.RefreshToken;
                mainUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

                await _mainUserRepository.CreateUser(mainUser, cancellationToken);
            }
            else
            {
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
                user.RefreshToken = request.RefreshToken;

                await _mainUserRepository.UpdateUser(user, cancellationToken);
            }

            return user!.Id.ToString();
        }
    }
}
