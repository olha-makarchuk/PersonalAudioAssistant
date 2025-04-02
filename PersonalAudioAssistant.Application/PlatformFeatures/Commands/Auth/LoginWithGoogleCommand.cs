using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth
{
    public class LoginWithGoogleCommand : IRequest
    {
        public string? Email { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand>
    {
        private readonly IMainUserRepository _mainUserRepository;
        private readonly IConfiguration _configuration;

        public LoginWithGoogleCommandHandler(IMainUserRepository mainUserRepository, IConfiguration configuration)
        {
            _mainUserRepository = mainUserRepository;
            _configuration = configuration;
        }

        public async Task Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken = default)
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

                await _mainUserRepository.CreateUser(mainUser, cancellationToken);
            }
            else
            {
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
                user.RefreshToken = request.RefreshToken;

                await _mainUserRepository.UpdateUser(user, cancellationToken);
            }
        }
    }
}
