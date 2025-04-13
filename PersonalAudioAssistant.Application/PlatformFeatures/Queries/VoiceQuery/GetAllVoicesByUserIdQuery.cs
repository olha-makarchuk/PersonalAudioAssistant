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
                    Description = voice.Description,
                    Age = voice.Age,
                    Gender = voice.Gender,
                    Id = voice.Id.ToString(),
                    Name = voice.Name,
                    URL = voice.URL,
                    UseCase = voice.UseCase,
                    UserId = voice.UserId,
                    VoiceId = voice.VoiceId
                }).ToList();

                return voiceResponse;
            }
        }
    }
}
