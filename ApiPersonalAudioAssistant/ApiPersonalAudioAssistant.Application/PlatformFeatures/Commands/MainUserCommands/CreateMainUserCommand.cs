
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MainUserCommands
{
    public class CreateMainUserCommand : IRequest<Unit>
    {
        public required string? Id { get; set; }
        public string? Email { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }

    public class CreateMainUserCommandHandler : IRequestHandler<CreateMainUserCommand, Unit>
    {
        private readonly IMainUserRepository _mainUserRepository;

        public CreateMainUserCommandHandler(IMainUserRepository mainUserRepository)
        {
            _mainUserRepository = mainUserRepository;
        }

        public async Task<Unit> Handle(CreateMainUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = new MainUser()
            {
                Email = request.Email,
                Id = Guid.NewGuid(),
                PasswordSalt = request.PasswordSalt,
                PasswordHash = request.PasswordHash,
                RefreshToken = request.RefreshToken,
                RefreshTokenExpiryTime = request.RefreshTokenExpiryTime
            };

            await _mainUserRepository.CreateUser(user, cancellationToken);

            return Unit.Value;
        }
    }
}
