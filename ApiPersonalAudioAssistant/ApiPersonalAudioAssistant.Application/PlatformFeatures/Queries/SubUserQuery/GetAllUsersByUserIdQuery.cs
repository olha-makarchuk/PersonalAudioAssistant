using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Contracts.SubUser;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class GetAllUsersByUserIdQuery : IRequest<List<SubUserResponse>>
    {
        public required string UserId { get; set; }

        public class GetAllUsersByUserIdQueryHandler : IRequestHandler<GetAllUsersByUserIdQuery, List<SubUserResponse>>
        {
            private readonly ISubUserRepository _subUserRepository;
            public GetAllUsersByUserIdQueryHandler(ISubUserRepository subUserRepository)
            {
                _subUserRepository = subUserRepository;
            }
            public async Task<List<SubUserResponse>> Handle(GetAllUsersByUserIdQuery query, CancellationToken cancellationToken)
            {
                var users = await _subUserRepository.GetAllUsersByUserId(query.UserId, cancellationToken);

                var userResponses = users.Select(user => new SubUserResponse()
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName,
                    StartPhrase = user.StartPhrase,
                    UserId = user.UserId,
                    EndPhrase = user.EndPhrase,
                    EndTime = user.EndTime,
                    UserVoice = user.UserVoice,
                    VoiceId = user.VoiceId,
                    PasswordHash = user.PasswordHash,
                    PhotoPath = user.PhotoPath
                }).ToList();

                return userResponses;
            }
        }
    }
}
