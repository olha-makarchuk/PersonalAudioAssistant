using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;
using System.Collections;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdateSubUserCommand : IRequest<Unit>
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public string? VoiceId { get; set; }
        //public List<double> UserVoice { get; set; }
        public byte[]? UserVoice { get; set; } = Array.Empty<byte>();
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
        public string? PhotoPath { get; set; }
    }

    public class UpdateSubUserCoomandHandler : IRequestHandler<UpdateSubUserCommand, Unit>
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

        public async Task<Unit> Handle(UpdateSubUserCommand request, CancellationToken cancellationToken = default)
        {
            var userExist = await _subUserRepository.GetUserByIdAsync(request.Id, cancellationToken);

            if (userExist == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            if (request.StartPhrase != null && userExist.StartPhrase!= request.StartPhrase)
            {
                var userByStartPhrase = await _subUserRepository.GetUserByStartPhraseAsync(request.UserId, request.StartPhrase, cancellationToken);
                if (userByStartPhrase != null)
                {
                    throw new Exception("Користувач із цією стартовою вразою вже існує");
                }
            }

            userExist.UserName = request.UserName ?? userExist.UserName;
            userExist.StartPhrase = request.StartPhrase ?? userExist.StartPhrase;
            userExist.EndPhrase = request.EndPhrase ?? userExist.EndPhrase;
            userExist.EndTime = request.EndTime ?? userExist.EndTime;
            userExist.VoiceId = request.VoiceId ?? userExist.VoiceId;

            if(request.UserVoice != Array.Empty<byte>())
            {
                var stream = new MemoryStream(request.UserVoice); 
                await _apiClientVoiceEmbedding.CreateVoiceEmbedding(stream);

                userExist.UserVoice = await _apiClientVoiceEmbedding.CreateVoiceEmbedding(stream);
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

            if(userExist.PasswordHash == Array.Empty<byte>() && !String.IsNullOrEmpty(request.NewPassword))
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
