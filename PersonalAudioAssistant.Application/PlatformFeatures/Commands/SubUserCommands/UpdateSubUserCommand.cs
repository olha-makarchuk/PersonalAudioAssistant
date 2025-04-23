using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdateSubUserCommand : IRequest
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public string VoiceId { get; set; }
        //public List<double> UserVoice { get; set; }
        public Stream UserVoice { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
        public string PhotoPath { get; set; }
    }

    public class UpdateSubUserCoomandHandler : IRequestHandler<UpdateSubUserCommand>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly PasswordManager _passwordManager;
        private readonly ApiClientVoiceEmbedding _apiClientVoiceEmbedding;
        private readonly IBlobStorage _blobStorage;

        public UpdateSubUserCoomandHandler(ISubUserRepository subUserRepository, PasswordManager passwordManager, IBlobStorage blobStorage, ApiClientVoiceEmbedding apiClientVoiceEmbedding)
        {
            _subUserRepository = subUserRepository;
            _passwordManager = passwordManager;
            _blobStorage = blobStorage;
            _apiClientVoiceEmbedding = apiClientVoiceEmbedding;
        }

        public async Task Handle(UpdateSubUserCommand request, CancellationToken cancellationToken = default)
        {
            if(request.StartPhrase != null)
            {
                var userByStartPhrase = await _subUserRepository.GetUserByStartPhraseAsync(request.UserId, request.StartPhrase, cancellationToken);
                if (userByStartPhrase != null)
                {
                    throw new Exception("Користувач із цією стартовою вразою вже існує");
                }
            }

            var userExist = await _subUserRepository.GetUserByIdAsync(request.Id, cancellationToken);

            if (userExist == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            userExist.UserName = request.UserName ?? userExist.UserName;
            userExist.StartPhrase = request.StartPhrase ?? userExist.StartPhrase;
            userExist.EndPhrase = request.EndPhrase ?? userExist.EndPhrase;
            userExist.EndTime = request.EndTime ?? userExist.EndTime;
            userExist.VoiceId = request.VoiceId ?? userExist.VoiceId;

            if(request.UserVoice != null)
            {
                userExist.UserVoice = await _apiClientVoiceEmbedding.CreateVoiceEmbedding(request.UserVoice);
            }

            if (request.Password != null && request.NewPassword != null)
            {
                if (!_passwordManager.VerifyPasswordHash(request.Password, userExist.PasswordHash!, userExist.PasswordSalt!))
                {
                    throw new Exception("Не вірний пароль");
                }

                _passwordManager.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                userExist.PasswordHash = passwordHash;
                userExist.PasswordSalt = passwordSalt;
            }

            if(userExist.PasswordHash == null && request.NewPassword != null)
            {
                _passwordManager.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                userExist.PasswordHash = passwordHash;
                userExist.PasswordSalt = passwordSalt;
            }

            await _subUserRepository.UpdateUser(userExist, cancellationToken);
        }
    }
}
