using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdateSubUserCoomand : IRequest
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
        public string? VoiceId { get; set; }
        public List<double>? UserVoice { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
    }

    public class UpdateSubUserCoomandHandler : IRequestHandler<UpdateSubUserCoomand>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IConfiguration _configuration;

        public UpdateSubUserCoomandHandler(ISubUserRepository subUserRepository, IConfiguration configuration)
        {
            _subUserRepository = subUserRepository;
            _configuration = configuration;
        }

        public async Task Handle(UpdateSubUserCoomand request, CancellationToken cancellationToken = default)
        {
            var user = await _subUserRepository.GetUserByStartPhraseAsync(request.UserId, request.StartPhrase, cancellationToken);
            if (user != null)
            {
                throw new Exception("User with this start phrase already exists.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Не вірний пароль");
            }

            CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            var newUser = new SubUser()
            {
                UserName = request.UserName,
                StartPhrase = request.StartPhrase,
                EndPhrase = request.EndPhrase,
                EndTime = request.EndTime,
                VoiceId = request.VoiceId,
                //UserVoice = request.UserVoice,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            await _subUserRepository.UpdateUser(newUser, cancellationToken);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
