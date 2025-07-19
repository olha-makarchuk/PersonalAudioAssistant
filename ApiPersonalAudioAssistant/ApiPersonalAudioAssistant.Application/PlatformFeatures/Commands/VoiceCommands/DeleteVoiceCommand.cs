using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands
{
    public class DeleteVoiceCommand : IRequest<Unit>
    {
        public required string Id { get; set; }
        public required string IdElevenlabs { get; set; }
    }

    public class DeleteVoiceCommandHandler : IRequestHandler<DeleteVoiceCommand, Unit>
    {
        private readonly IVoiceRepository _voiceRepository;
        private readonly ElevenlabsApi _elevenLabsApi = new ElevenlabsApi();

        public DeleteVoiceCommandHandler(IVoiceRepository voiceRepository)
        {
            _voiceRepository = voiceRepository;
        }

        public async Task<Unit> Handle(DeleteVoiceCommand request, CancellationToken cancellationToken = default)
        { 
            await _elevenLabsApi.DeleteVoiceAsync(request.IdElevenlabs);

            var user = await _voiceRepository.GetVoiceByIdAsync(request.Id, cancellationToken);
            if (user == null)
            {
                throw new Exception("Voice with this Id not exists.");
            }

            await _voiceRepository.DeleteVoiceById(request.Id, cancellationToken);

            return Unit.Value;
        }
    }
}
