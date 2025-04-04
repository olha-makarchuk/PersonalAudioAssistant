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
                var user = await _mainUserRepository.GetUserByEmailAsync(query.Name, cancellationToken);
                if(user == null)
                {
                    throw new Exception("User not found");
                }

                return user;
            }
        }
    }
}
