using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands
{
    public class UpdateVoiceCommand : IRequest
    {
        public string VoiceId { get; set; } 
        public string? UserId { get; set; }
    }

    public class UpdateVoiceCoomandHandler : IRequestHandler<UpdateVoiceCommand>
    {
        private readonly IVoiceRepository _voiceRepository;

        public UpdateVoiceCoomandHandler(IVoiceRepository voiceRepository)
        {
            _voiceRepository = voiceRepository;
        }

        public async Task Handle(UpdateVoiceCommand request, CancellationToken cancellationToken = default)
        {
            var voice = await _voiceRepository.GetVoiceByIdAsync(request.VoiceId, cancellationToken);

            if (voice == null)
            {
                throw new Exception("Голосу не існує");
            }

            voice.UserId = request.UserId;

            await _voiceRepository.UpdateVoiceAsync(voice, cancellationToken);
        }
    }
}
