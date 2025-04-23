using MediatR;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands
{
    public class DeleteVoiceCommand : IRequest
    {
        public required string Id { get; set; }
    }

    public class DeleteVoiceCommandHandler : IRequestHandler<DeleteVoiceCommand>
    {
        private readonly IVoiceRepository _voiceRepository;

        public DeleteVoiceCommandHandler(IVoiceRepository voiceRepository)
        {
            _voiceRepository = voiceRepository;
        }

        public async Task Handle(DeleteVoiceCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _voiceRepository.GetVoiceByIdAsync(request.Id, cancellationToken);
            if (user == null)
            {
                throw new Exception("Voice with this Id not exists.");
            }

            await _voiceRepository.DeleteVoiceById(request.Id, cancellationToken);
        }
    }
}
