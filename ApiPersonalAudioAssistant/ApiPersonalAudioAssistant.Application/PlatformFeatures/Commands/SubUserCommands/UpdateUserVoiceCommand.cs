using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdateUserVoiceCommand : IRequest<Unit>
    {
        public string UserId { get; set; }
        public byte[] UserVoice { get; set; }
    }

    public class UpdateUserVoiceCommandHandler : IRequestHandler<UpdateUserVoiceCommand, Unit>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly PasswordManager _passwordManager;
        private readonly ApiClientVoiceEmbedding _apiClientVoiceEmbedding;
        private readonly IBlobStorage _blobStorage;

        public UpdateUserVoiceCommandHandler(ISubUserRepository subUserRepository, PasswordManager passwordManager, IBlobStorage blobStorage, ApiClientVoiceEmbedding apiClientVoiceEmbedding)
        {
            _subUserRepository = subUserRepository;
            _passwordManager = passwordManager;
            _blobStorage = blobStorage;
            _apiClientVoiceEmbedding = apiClientVoiceEmbedding;
        }

        public async Task<Unit> Handle(UpdateUserVoiceCommand request, CancellationToken cancellationToken = default)
        {
            var userExist = await _subUserRepository.GetUserByIdAsync(request.UserId, cancellationToken);

            if (userExist == null)
            {
                throw new Exception("Користувача не знайдено");
            }

            if (request.UserVoice != null)
            {
                var stream = new MemoryStream(request.UserVoice);

                userExist.UserVoice = await _apiClientVoiceEmbedding.CreateVoiceEmbedding(stream);
            }

            await _subUserRepository.UpdateUser(userExist, cancellationToken);
            return Unit.Value;
        }
    }
}
