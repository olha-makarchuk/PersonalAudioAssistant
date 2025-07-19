using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class DeleteSubUserCommand: IRequest<Unit>
    {
        public required string UserId { get; set; }
    }

    public class DeleteSubUserCommandHandler : IRequestHandler<DeleteSubUserCommand, Unit>
    {
        private readonly ISubUserRepository _subUserRepository;

        public DeleteSubUserCommandHandler(ISubUserRepository subUserRepository)
        {
            _subUserRepository = subUserRepository;
        }

        public async Task<Unit> Handle(DeleteSubUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _subUserRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new Exception("User with this Id not exists.");
            }

            await _subUserRepository.DeleteUser(request.UserId, cancellationToken);
            return Unit.Value;
        }
    }
}
