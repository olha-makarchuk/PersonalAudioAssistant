using ApiPersonalAudioAssistant.Application.Interfaces;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MainUserCommands
{
    public class UpdateMainUserCommand : IRequest<Unit>
    {
        public required string? Id { get; set; }
        public string? Email { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }

    public class UpdateMainUserCommandHandler : IRequestHandler<UpdateMainUserCommand, Unit>
    {
        private readonly IMainUserRepository _mainUserRepository;

        public UpdateMainUserCommandHandler(IMainUserRepository mainUserRepository)
        {
            _mainUserRepository = mainUserRepository;
        }

        public async Task<Unit> Handle(UpdateMainUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _mainUserRepository.GetUserByIdAsync(request.Id, cancellationToken);
            user.RefreshToken = request.RefreshToken;
            user.RefreshTokenExpiryTime = request.RefreshTokenExpiryTime;
            user.PasswordHash = request.PasswordHash;
            user.PasswordSalt = request.PasswordSalt;
            user.Email = request.Email;

            await _mainUserRepository.UpdateUser(user, cancellationToken);

            return Unit.Value;
        }
    }
}
