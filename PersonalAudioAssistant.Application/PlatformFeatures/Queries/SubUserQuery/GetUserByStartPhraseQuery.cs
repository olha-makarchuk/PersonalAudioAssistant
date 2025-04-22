using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.SubUser;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class GetUserByStartPhraseQuery : IRequest<SubUserResponse>
    {
        public string StartPhrase { get; set; } = null!;

        public class GetUserByStartPhraseQueryHandler : IRequestHandler<GetUserByStartPhraseQuery, SubUserResponse>
        {
            private readonly ISubUserRepository _subUserRepository;
            public GetUserByStartPhraseQueryHandler(ISubUserRepository subUserRepository)
            {
                _subUserRepository = subUserRepository;
            }
            public async Task<SubUserResponse> Handle(GetUserByStartPhraseQuery query, CancellationToken cancellationToken)
            {
                var user = await _subUserRepository.GetUserByNameAsync(query.StartPhrase, cancellationToken);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var userResponse = new SubUserResponse()
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
                };

                return userResponse;
            }
        }
    }
}
