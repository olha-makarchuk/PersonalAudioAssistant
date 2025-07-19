using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdatePasswordCommand : IRequest<Unit>
    {
        public string? Id { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
    }

    public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Unit>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly PasswordManager _passwordManager;

        public UpdatePasswordCommandHandler(ISubUserRepository subUserRepository, PasswordManager passwordManager)
        {
            _subUserRepository = subUserRepository;
            _passwordManager = passwordManager;
        }

        public async Task<Unit> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken = default)
        {
            var userExist = await _subUserRepository.GetUserByIdAsync(request.Id, cancellationToken);

            if (userExist == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            if (!String.IsNullOrEmpty(request.Password) && !String.IsNullOrEmpty(request.NewPassword))
            {
                if (!_passwordManager.VerifyPasswordHash(request.Password, userExist.PasswordHash!, userExist.PasswordSalt!))
                {
                    throw new Exception("Не вірний пароль");
                }

                _passwordManager.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                userExist.PasswordHash = passwordHash;
                userExist.PasswordSalt = passwordSalt;
            }

            if (userExist.PasswordHash == null && !String.IsNullOrEmpty(request.NewPassword))
            {
                _passwordManager.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                userExist.PasswordHash = passwordHash;
                userExist.PasswordSalt = passwordSalt;
            }

            await _subUserRepository.UpdateUser(userExist, cancellationToken);
            return Unit.Value;
        }
    }
}
