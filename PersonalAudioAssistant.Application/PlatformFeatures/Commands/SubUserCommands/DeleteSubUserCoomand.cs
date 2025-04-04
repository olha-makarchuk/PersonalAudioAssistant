using MediatR;
using Microsoft.Extensions.Configuration;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class DeleteSubUserCoomand: IRequest
    {
        public string? UserId { get; set; }
    }

    public class DeleteSubUserCoomandHandler : IRequestHandler<DeleteSubUserCoomand>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IConfiguration _configuration;

        public DeleteSubUserCoomandHandler(ISubUserRepository subUserRepository, IConfiguration configuration)
        {
            _subUserRepository = subUserRepository;
            _configuration = configuration;
        }

        public async Task Handle(DeleteSubUserCoomand request, CancellationToken cancellationToken = default)
        {
            var user = _subUserRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            if (user != null)
            {
                throw new Exception("User with this Id not exists.");
            }

            await _subUserRepository.DeleteUser(request.UserId, cancellationToken);
        }
    }
}
