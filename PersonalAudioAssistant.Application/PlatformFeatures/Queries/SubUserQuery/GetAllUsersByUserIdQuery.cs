
using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class GetAllUsersByUserIdQuery : IRequest<List<SubUser>>
    {
        public string? UserId { get; set; }

        public class GetAllUsersByUserIdQueryHandler : IRequestHandler<GetAllUsersByUserIdQuery, List<SubUser>>
        {
            private readonly ISubUserRepository _subUserRepository;
            public GetAllUsersByUserIdQueryHandler(ISubUserRepository subUserRepository)
            {
                _subUserRepository = subUserRepository;
            }
            public async Task<List<SubUser>> Handle(GetAllUsersByUserIdQuery query, CancellationToken cancellationToken)
            {
                var users = await _subUserRepository.GetAllUsersByUserId(query.UserId, cancellationToken);

                return users;
            }
        }
    }
}
