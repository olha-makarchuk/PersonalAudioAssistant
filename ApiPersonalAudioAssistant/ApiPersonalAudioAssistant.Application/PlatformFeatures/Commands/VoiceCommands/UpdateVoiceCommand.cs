using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands
{
    public class UpdateVoiceCommand : IRequest<Unit>
    {
        public string VoiceId { get; set; } 
        public string? UserId { get; set; }
    }

    public class UpdateVoiceCoomandHandler : IRequestHandler<UpdateVoiceCommand, Unit>
    {
        private readonly IVoiceRepository _voiceRepository;

        public UpdateVoiceCoomandHandler(IVoiceRepository voiceRepository)
        {
            _voiceRepository = voiceRepository;
        }

        public async Task<Unit> Handle(UpdateVoiceCommand request, CancellationToken cancellationToken = default)
        {
            var voice = await _voiceRepository.GetVoiceByIdAsync(request.VoiceId, cancellationToken);

            if (voice == null)
            {
                throw new Exception("Голосу не існує");
            }

            voice.UserId = request.UserId;

            await _voiceRepository.UpdateVoiceAsync(voice, cancellationToken);
            return Unit.Value;
        }
    }
}
