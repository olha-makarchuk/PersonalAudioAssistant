using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class DeletePasswordSubUserCommand : IRequest<Unit>
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    public class DeletePasswordSubUserCommandHandler : IRequestHandler<DeletePasswordSubUserCommand, Unit>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly PasswordManager _passwordManager;

        public DeletePasswordSubUserCommandHandler(ISubUserRepository subUserRepository, PasswordManager passwordManager)
        {
            _subUserRepository = subUserRepository;
            _passwordManager = passwordManager;
        }

        public async Task<Unit> Handle(DeletePasswordSubUserCommand request, CancellationToken cancellationToken = default)
        {
            var userExist = await _subUserRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            if (userExist == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            if (!_passwordManager.VerifyPasswordHash(request.Password, userExist.PasswordHash!, userExist.PasswordSalt!))
            {
                throw new Exception("Пароль не вірний");
            }

            userExist.PasswordHash = null;
            userExist.PasswordSalt = null;

            await _subUserRepository.UpdateUser(userExist, cancellationToken);

            return Unit.Value;
        }
    }
}
