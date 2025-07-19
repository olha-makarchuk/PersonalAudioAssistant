using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.Voice;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery
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
                if(string.IsNullOrEmpty(query.VoiceId))
                {
                    throw new Exception("Voice not found");
                }
                var voice = await _voiceRepository.GetVoiceByIdAsync(query.VoiceId, cancellationToken);
                if (voice == null)
                {
                    throw new Exception("Voice not found");
                }

                var voiceResponse = new VoiceResponse()
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
                };

                return voiceResponse;
            }
        }
    }
}
