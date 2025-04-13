using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.MainUser;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.MainUserQuery
{
    public class GetMainUserByEmailQuery : IRequest<MainUserResponse>
    {
        public required string Name { get; set; }

        public class GetMainUserByEmailQueryHandler : IRequestHandler<GetMainUserByEmailQuery, MainUserResponse>
        {
            private readonly IMainUserRepository _mainUserRepository;
            public GetMainUserByEmailQueryHandler(IMainUserRepository mainUserRepository)
            {
                _mainUserRepository = mainUserRepository;
            }
            public async Task<MainUserResponse> Handle(GetMainUserByEmailQuery query, CancellationToken cancellationToken)
            {
                var user = await _mainUserRepository.GetUserByEmailAsync(query.Name, cancellationToken);
                if(user == null)
                {
                    throw new Exception("User not found");
                }

                var userResponse = new MainUserResponse
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    PasswordSalt = user.PasswordSalt,
                    RefreshToken = user.RefreshToken,
                    RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
                };

                return userResponse;
            }
        }
    }
}
