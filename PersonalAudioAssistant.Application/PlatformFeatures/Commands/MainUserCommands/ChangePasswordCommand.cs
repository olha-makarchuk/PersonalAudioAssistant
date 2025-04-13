using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.MainUserCommands
{
    public class ChangePasswordCommand : IRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string NewPassword { get; set; }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
    {
        private readonly IMainUserRepository _mainUserRepository;
        private readonly PasswordManager _passwordManager;

        public ChangePasswordCommandHandler(IMainUserRepository mainUserRepository, PasswordManager passwordManager)
        {
            _mainUserRepository = mainUserRepository;
            _passwordManager = passwordManager;
        }

        public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new ArgumentException("не всі поля заповнені");
            }

            var user = await _mainUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);

            if (!_passwordManager.VerifyPasswordHash(request.Password, user.PasswordHash!, user.PasswordSalt!))
            {
                throw new Exception("Логін або пароль не вірні");
            }

            _passwordManager.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _mainUserRepository.UpdateUser(user, cancellationToken);
        }
    }
}
