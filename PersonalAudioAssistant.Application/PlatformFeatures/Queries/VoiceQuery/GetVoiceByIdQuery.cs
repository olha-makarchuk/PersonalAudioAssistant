using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.Voice;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery
{
    public class GetVoiceByIdQuery : IRequest<VoiceResponse>
    {
        public required string VoiceId { get; set; }

        public class GetVoiceByIdQueryHandler : IRequestHandler<GetVoiceByIdQuery, VoiceResponse>
        {
            private readonly IVoiceRepository _voiceRepository;
            public GetVoiceByIdQueryHandler(IVoiceRepository voiceRepository)
            {
                _voiceRepository = voiceRepository;
            }
            public async Task<VoiceResponse> Handle(GetVoiceByIdQuery query, CancellationToken cancellationToken)
            {
                var voice = await _voiceRepository.GetVoiceByIdAsync(query.VoiceId, cancellationToken);
                if (voice == null)
                {
                    throw new Exception("Voice not found");
                }

                var voiceResponse = new VoiceResponse()
                {
                    description = voice.Description,
                    age = voice.Age,
                    gender = voice.Gender,
                    id = voice.Id.ToString(),
                    name = voice.Name,
                    url = voice.URL,
                    useCase = voice.UseCase,
                    userId = voice.UserId,
                    voiceId = voice.VoiceId
                };

                return voiceResponse;
            }
        }
    }
}
