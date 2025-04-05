using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth
{
    public class LoginWithGoogleCommand : IRequest<string>
    {
        public string? Email { get; set; }
        public string? RefreshToken { get; set; }
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
            var RefreshTokenExpiryDays = double.Parse(_configuration["JWTKey:ExpiryDays"]);

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Не всі передані поля заповнені", nameof(request.Email));

            var user = await _mainUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);

            MainUser mainUser = new();

            if (user is null)
            {
                mainUser = new()
                {
                    Email = request.Email,
                    RefreshToken = request.RefreshToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
                };

                var appSettings = new AppSettings()
                {
                    Theme = "Light",
                    Payment = null,
                    MinTokenThreshold = -1,
                    ChargeAmount = -1
                };

                await _appSettingsRepository.AddSettingsAsync(appSettings, cancellationToken);

                await _mainUserRepository.CreateUser(mainUser, cancellationToken);
            }
            else
            {
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
                user.RefreshToken = request.RefreshToken;

                await _mainUserRepository.UpdateUser(user, cancellationToken);
            }

            return user.Id.ToString();
        }
    }
}
