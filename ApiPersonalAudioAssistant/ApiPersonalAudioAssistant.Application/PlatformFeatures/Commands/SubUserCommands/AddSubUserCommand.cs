using MediatR;
using Microsoft.Extensions.Configuration;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;
using ApiPersonalAudioAssistant.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class AddSubUserCommand : IRequest<string>
    {
        public required string UserName { get; set; }
        public required string StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public required string VoiceId { get; set; }
        public required List<double> UserVoice { get; set; }
        public string? Password { get; set; }
        public required string UserId { get; set; }
        public IFormFile Photo { get; set; }
    }

    public class AddSubUserCommandHandler : IRequestHandler<AddSubUserCommand, string>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IVoiceRepository _voiceRepository;
        private readonly IConfiguration _configuration;
        private readonly IBlobStorage _blobStorage;
        private readonly PasswordManager _passwordManager;

        public AddSubUserCommandHandler(ISubUserRepository subUserRepository, IConfiguration configuration, PasswordManager passwordManager, IBlobStorage blobStorage, IVoiceRepository voiceRepository)
        {
            _subUserRepository = subUserRepository;
            _configuration = configuration;
            _passwordManager = passwordManager;
            _blobStorage = blobStorage;
            _voiceRepository = voiceRepository;
        }

        public async Task<string> Handle(AddSubUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _subUserRepository.GetUserByStartPhraseAsync(request.UserId, request.StartPhrase, cancellationToken);
            if (user != null)
            {
                throw new Exception("Користувач з такою стартовою фразою вже існує");
            }

            var newUser = new SubUser()
            {
                UserName = request.UserName,
                StartPhrase = request.StartPhrase,
                EndPhrase = request.EndPhrase,
                EndTime = request.EndTime,
                VoiceId = request.VoiceId,
                UserVoice = request.UserVoice,
                PhotoPath = "",
                UserId = request.UserId
            };

            var voiceId = await _voiceRepository.GetVoiceByIdAsync(request.VoiceId, cancellationToken);
            var textToSpeech = new ElevenlabsApi();
            var audioBytesTask = textToSpeech.ConvertTextToSpeechAsync(voiceId.VoiceId, $"Чим я можу вам допомогти, {newUser.UserName}");
            string fileNameAudio = $"{newUser.Id}.wav";

            if (request.Password != null)
            {
                _passwordManager.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                newUser.PasswordHash = passwordHash;
                newUser.PasswordSalt = passwordSalt;
            }
            await _subUserRepository.AddUser(newUser, cancellationToken);

            var extension = Path.GetExtension(request.Photo.FileName);
            using var stream = request.Photo.OpenReadStream();
            string fileName = $"{newUser.Id}{extension}";
            await _blobStorage.PutContextAsync(fileName, stream, BlobContainerType.UserImage);

            newUser.PhotoPath = $"https://audioassistantblob.blob.core.windows.net/user-image/{fileName}";
            await _subUserRepository.UpdateUser(newUser, cancellationToken);

            if (audioBytesTask.Result != null && audioBytesTask.Result.Length > 0)
            {
                using (var streamAudio = new MemoryStream(audioBytesTask.Result))
                {
                    var taskBlob = _blobStorage.PutContextAsync(fileNameAudio, streamAudio, BlobContainerType.FirstMessage);
                }
            }

            return newUser.Id.ToString();
        }
    }
}