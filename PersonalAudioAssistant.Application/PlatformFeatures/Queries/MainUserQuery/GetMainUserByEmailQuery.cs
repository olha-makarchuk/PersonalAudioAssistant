using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.MainUserQuery
{

    public class GetMainUserByEmailQuery : IRequest<MainUser>
    {
        public string Name { get; set; } = null!;

        public class GetMainUserByEmailQueryHandler : IRequestHandler<GetMainUserByEmailQuery, MainUser>
        {
            private readonly IMainUserRepository _mainUserRepository;
            public GetMainUserByEmailQueryHandler(IMainUserRepository mainUserRepository)
            {
                _mainUserRepository = mainUserRepository;
            }
            public async Task<MainUser> Handle(GetMainUserByEmailQuery query, CancellationToken cancellationToken)
            {
                return await _mainUserRepository.GetUserByEmailAsync(query.Name, cancellationToken);
                                        //?? throw new NotFoundException("User not found");
            }
        }
    }
}
