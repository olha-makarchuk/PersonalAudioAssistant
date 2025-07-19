using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;
using MediatR;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdateVoiceActingCommand : IRequest<Unit>
    {
        public string? Id { get; set; }
        public string? VoiceId { get; set; }
    }

    public class UpdateVoiceActingCommandHandler : IRequestHandler<UpdateVoiceActingCommand, Unit>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IVoiceRepository _voiceRepository;
        private readonly IBlobStorage _blobStorage;

        public UpdateVoiceActingCommandHandler(ISubUserRepository subUserRepository, IBlobStorage blobStorage, IVoiceRepository voiceRepository)
        {
            _subUserRepository = subUserRepository;
            _blobStorage = blobStorage;
            _voiceRepository = voiceRepository;
        }

        public async Task<Unit> Handle(UpdateVoiceActingCommand request, CancellationToken cancellationToken = default)
        {
            var userExist = await _subUserRepository.GetUserByIdAsync(request.Id, cancellationToken);

            if (userExist == null)
            {
                throw new Exception("Користувача не знайдено");
            }
            userExist.VoiceId = request.VoiceId;

            var voiceId = await _voiceRepository.GetVoiceByIdAsync(request.VoiceId, cancellationToken);
            var textToSpeech = new ElevenlabsApi();
            var audioBytesTask = await textToSpeech.ConvertTextToSpeechAsync(voiceId.VoiceId, $"Чим я можу вам допомогти, {userExist.UserName}");
            string fileNameAudio = $"{userExist.Id}.wav";

            var exists = await _blobStorage.FileExistsAsync(fileNameAudio, BlobContainerType.FirstMessage);
            if (exists)
            {
                await _blobStorage.DeleteAsync(fileNameAudio, BlobContainerType.FirstMessage);
            }

            if (audioBytesTask != null && audioBytesTask.Length > 0)
            {
                using (var streamAudio = new MemoryStream(audioBytesTask))
                {
                    var taskBlob = _blobStorage.PutContextAsync(fileNameAudio, streamAudio, BlobContainerType.FirstMessage);
                }
            }

            await _subUserRepository.UpdateUser(userExist, cancellationToken);
            return Unit.Value;
        }
    }
}
