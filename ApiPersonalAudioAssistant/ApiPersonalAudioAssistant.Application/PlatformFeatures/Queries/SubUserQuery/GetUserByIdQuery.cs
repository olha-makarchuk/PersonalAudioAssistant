using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.SubUser;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class GetUserByIdQuery : IRequest<SubUserResponse>
    {
        public required string UserId { get; set; } 

        public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, SubUserResponse>
        {
            private readonly ISubUserRepository _subUserRepository;
            public GetUserByIdQueryHandler(ISubUserRepository subUserRepository)
            {
                _subUserRepository = subUserRepository;
            }
            public async Task<SubUserResponse> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
            {
                var user = await _subUserRepository.GetUserByIdAsync(query.UserId, cancellationToken);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var userResponse = new SubUserResponse()
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName,
                    UserId = user.UserId,
                    StartPhrase = user.StartPhrase,
                    EndPhrase = user.EndPhrase,
                    EndTime = user.EndTime,
                    UserVoice = user.UserVoice,
                    VoiceId = user.VoiceId,
                    PasswordHash = user.PasswordHash,
                    PhotoPath = user.PhotoPath
                };

                return userResponse;
            }
        }
    }
}
