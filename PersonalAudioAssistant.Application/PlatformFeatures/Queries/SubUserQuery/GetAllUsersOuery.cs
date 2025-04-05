
using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class GetAllUsersOuery : IRequest<List<SubUser>>
    {
        public class GetAllUsersOueryHandler : IRequestHandler<GetAllUsersOuery, List<SubUser>>
        {
            private readonly ISubUserRepository _subUserRepository;
            public GetAllUsersOueryHandler(ISubUserRepository subUserRepository)
            {
                _subUserRepository = subUserRepository;
            }
            public async Task<List<SubUser>> Handle(GetAllUsersOuery query, CancellationToken cancellationToken)
            {
                var users = await _subUserRepository.GetAllUsers(cancellationToken);

                return users;
            }
        }
    }
}
