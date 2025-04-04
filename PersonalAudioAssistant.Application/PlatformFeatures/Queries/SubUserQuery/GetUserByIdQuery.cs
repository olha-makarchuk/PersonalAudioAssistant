using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.MainUserQuery;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class GetUserByIdQuery : IRequest<SubUser>
    {
        public string UserId { get; set; } = null!;

        public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, SubUser>
        {
            private readonly ISubUserRepository _subUserRepository;
            public GetUserByIdQueryHandler(ISubUserRepository subUserRepository)
            {
                _subUserRepository = subUserRepository;
            }
            public async Task<SubUser> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
            {
                var user = await _subUserRepository.GetUserByIdAsync(query.UserId, cancellationToken);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                return user;
            }
        }
    }
}
