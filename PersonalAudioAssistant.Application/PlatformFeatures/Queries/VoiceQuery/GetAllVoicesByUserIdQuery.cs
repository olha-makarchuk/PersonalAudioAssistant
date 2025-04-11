using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery
{
    public class GetAllVoicesByUserIdQuery : IRequest<List<Voice>>
    {
        public string? UserId { get; set; }

        public class GetAllVoicesByUserIdQueryHandler : IRequestHandler<GetAllVoicesByUserIdQuery, List<Voice>>
        {
            private readonly IVoiceRepository _voiceRepository;
            public GetAllVoicesByUserIdQueryHandler(IVoiceRepository voiceRepository)
            {
                _voiceRepository = voiceRepository;
            }
            public async Task<List<Voice>> Handle(GetAllVoicesByUserIdQuery query, CancellationToken cancellationToken)
            {
                var voices = await _voiceRepository.GetAllVoicesByUserIdAsync(query.UserId, cancellationToken);

                return voices;
            }
        }
    }
}
