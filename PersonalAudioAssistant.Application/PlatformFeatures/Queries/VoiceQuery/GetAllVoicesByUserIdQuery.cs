using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.Voice;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery
{
    public class GetAllVoicesByUserIdQuery : IRequest<List<VoiceResponse>>
    {
        public required string UserId { get; set; }

        public class GetAllVoicesByUserIdQueryHandler : IRequestHandler<GetAllVoicesByUserIdQuery, List<VoiceResponse>>
        {
            private readonly IVoiceRepository _voiceRepository;
            public GetAllVoicesByUserIdQueryHandler(IVoiceRepository voiceRepository)
            {
                _voiceRepository = voiceRepository;
            }
            public async Task<List<VoiceResponse>> Handle(GetAllVoicesByUserIdQuery query, CancellationToken cancellationToken)
            {
                var voices = await _voiceRepository.GetAllVoicesByUserIdAsync(query.UserId, cancellationToken);

                var voiceResponse = voices.Select(voice => new VoiceResponse()
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
                }).ToList();

                return voiceResponse;
            }
        }
    }
}
