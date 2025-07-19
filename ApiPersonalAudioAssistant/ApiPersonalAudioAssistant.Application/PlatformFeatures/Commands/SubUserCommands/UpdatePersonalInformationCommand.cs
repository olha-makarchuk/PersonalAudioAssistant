using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdatePersonalInformationCommand : IRequest<Unit>
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? StartPhrase { get; set; }
        public string? EndPhrase { get; set; }
        public string? EndTime { get; set; }
    }

    public class UpdatePersonalInformationCommandHandler : IRequestHandler<UpdatePersonalInformationCommand, Unit>
    {
        private readonly ISubUserRepository _subUserRepository;

        public UpdatePersonalInformationCommandHandler(ISubUserRepository subUserRepository)
        {
            _subUserRepository = subUserRepository;
        }

        public async Task<Unit> Handle(UpdatePersonalInformationCommand request, CancellationToken cancellationToken = default)
        {
            var userExist = await _subUserRepository.GetUserByIdAsync(request.Id, cancellationToken);

            if (userExist == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            if (request.StartPhrase != null && userExist.StartPhrase != request.StartPhrase)
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

            await _subUserRepository.UpdateUser(userExist, cancellationToken);
            return Unit.Value;
        }
    }
}
