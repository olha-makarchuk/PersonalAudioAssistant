using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class GetUserByStartPhraseQuery : IRequest<SubUser>
    {
        public string StartPhrase { get; set; } = null!;

        public class GetUserByStartPhraseQueryHandler : IRequestHandler<GetUserByStartPhraseQuery, SubUser>
        {
            private readonly ISubUserRepository _subUserRepository;
            public GetUserByStartPhraseQueryHandler(ISubUserRepository subUserRepository)
            {
                _subUserRepository = subUserRepository;
            }
            public async Task<SubUser> Handle(GetUserByStartPhraseQuery query, CancellationToken cancellationToken)
            {
                var user = await _subUserRepository.GetUserByNameAsync(query.StartPhrase, cancellationToken);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                return user;
            }
        }
    }
}
