using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class DeleteSubUserCoomand: IRequest
    {
        public required string UserId { get; set; }
    }

    public class DeleteSubUserCoomandHandler : IRequestHandler<DeleteSubUserCoomand>
    {
        private readonly ISubUserRepository _subUserRepository;

        public DeleteSubUserCoomandHandler(ISubUserRepository subUserRepository)
        {
            _subUserRepository = subUserRepository;
        }

        public async Task Handle(DeleteSubUserCoomand request, CancellationToken cancellationToken = default)
        {
            var user = await _subUserRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new Exception("User with this Id not exists.");
            }

            await _subUserRepository.DeleteUser(request.UserId, cancellationToken);
        }
    }
}
