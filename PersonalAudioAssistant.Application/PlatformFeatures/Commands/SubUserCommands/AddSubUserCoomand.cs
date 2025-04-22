using Google.Apis.Drive.v3.Data;
using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class AddSubUserCoomand : IRequest<string>
    {
        public required string UserName { get; set; }
        public required string StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public required string VoiceId { get; set; }
        public required List<double> UserVoice { get; set; }
        public string? Password { get; set; }
        public required string UserId { get; set; }
        public required string PhotoPath { get; set; }
    }

    public class AddSubUserCoomandHandler : IRequestHandler<AddSubUserCoomand, string>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IConfiguration _configuration;
        private readonly IBlobStorage _blobStorage;
        private readonly PasswordManager _passwordManager;

        public AddSubUserCoomandHandler(ISubUserRepository subUserRepository, IConfiguration configuration, PasswordManager passwordManager, IBlobStorage blobStorage)
        {
            _subUserRepository = subUserRepository;
            _configuration = configuration;
            _passwordManager = passwordManager;
            _blobStorage = blobStorage;
        }

        public async Task<string> Handle(AddSubUserCoomand request, CancellationToken cancellationToken = default)
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

            if (request.Password != null)
            {
                _passwordManager.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                newUser.PasswordHash = passwordHash;
                newUser.PasswordSalt = passwordSalt;
            }
            await _subUserRepository.AddUser(newUser, cancellationToken);

            var extension = Path.GetExtension(request.PhotoPath);
            using var stream = System.IO.File.OpenRead(request.PhotoPath);
            string fileName = $"{newUser.Id}{extension}";
            await _blobStorage.PutContextAsync(fileName, stream, BlobContainerType.UserImage);
            newUser.PhotoPath = $"https://audioassistantblob.blob.core.windows.net/user-image/{fileName}";
            await _subUserRepository.UpdateUser(newUser, cancellationToken);

            return newUser.Id.ToString();
        }
    }
}