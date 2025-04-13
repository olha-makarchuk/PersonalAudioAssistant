using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdateSubUserCoomand : IRequest
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public required string VoiceId { get; set; }
        public required List<double> UserVoice { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
    }

    public class UpdateSubUserCoomandHandler : IRequestHandler<UpdateSubUserCoomand>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly PasswordManager _passwordManager;

        public UpdateSubUserCoomandHandler(ISubUserRepository subUserRepository, PasswordManager passwordManager)
        {
            _subUserRepository = subUserRepository;
            _passwordManager = passwordManager;
        }

        public async Task Handle(UpdateSubUserCoomand request, CancellationToken cancellationToken = default)
        {
            var user = await _subUserRepository.GetUserByStartPhraseAsync(request.UserId, request.StartPhrase, cancellationToken);
            if (user != null)
            {
                throw new Exception("Користувач із цією стартовою вразою вже існує");
            }

            var newUser = new SubUser()
            {
                UserId = request.UserId,
                UserName = request.UserName,
                StartPhrase = request.StartPhrase,
                EndPhrase = request.EndPhrase,
                EndTime = request.EndTime,
                VoiceId = request.VoiceId,
                UserVoice = request.UserVoice
            };

            if (request.Password != null && request.NewPassword != null)
            {
                if (!_passwordManager.VerifyPasswordHash(request.Password, user!.PasswordHash!, user.PasswordSalt!))
                {
                    throw new Exception("Не вірний пароль");
                }
                _passwordManager.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

                newUser.PasswordHash = passwordHash;
                newUser.PasswordSalt = passwordSalt;
            }

            await _subUserRepository.UpdateUser(newUser, cancellationToken);
        }
    }
}
