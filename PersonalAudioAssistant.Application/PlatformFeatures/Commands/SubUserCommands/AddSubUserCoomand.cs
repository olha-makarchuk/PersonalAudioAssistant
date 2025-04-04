using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class AddSubUserCoomand : IRequest
    {
        public string? UserName { get; set; }
        public string? StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public string? VoiceId { get; set; }
        public byte[]? UserVoice { get; set; }
        public string? Password { get; set; }
    }

    public class AddSubUserCoomandHandler : IRequestHandler<AddSubUserCoomand>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IConfiguration _configuration;

        public AddSubUserCoomandHandler(ISubUserRepository subUserRepository, IConfiguration configuration)
        {
            _subUserRepository = subUserRepository;
            _configuration = configuration;
        }

        public async Task Handle(AddSubUserCoomand request, CancellationToken cancellationToken = default)
        {
            var user = _subUserRepository.GetUserByStartPhraseAsync(request.StartPhrase, cancellationToken);
            if (user != null)
            {
                throw new Exception("User with this start phrase already exists.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var newUser = new SubUser()
            {
                UserName = request.UserName,
                StartPhrase = request.StartPhrase,
                EndPhrase = request.EndPhrase,
                EndTime = request.EndTime,
                VoiceId = request.VoiceId,
                UserVoice = request.UserVoice,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            await _subUserRepository.AddUser(newUser, cancellationToken);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}