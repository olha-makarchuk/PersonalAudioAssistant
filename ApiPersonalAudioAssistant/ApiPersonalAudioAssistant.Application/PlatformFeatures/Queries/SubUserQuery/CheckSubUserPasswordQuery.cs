using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery
{
    public class CheckSubUserPasswordQuery: IRequest<bool>
    {
        public required string UserId { get; set; }
        public required string Password { get; set; }

        public class CheckSubUserPasswordQueryHandler : IRequestHandler<CheckSubUserPasswordQuery, bool>
        {
            private readonly ISubUserRepository _subUserRepository;
            private readonly PasswordManager _passwordManager;

            public CheckSubUserPasswordQueryHandler(ISubUserRepository subUserRepository, PasswordManager passwordManager)
            {
                _subUserRepository = subUserRepository;
                _passwordManager = passwordManager;
            }

            public async Task<bool> Handle(CheckSubUserPasswordQuery query, CancellationToken cancellationToken)
            {
                var userExist = await _subUserRepository.GetUserByIdAsync(query.UserId, cancellationToken);
                if (userExist == null)
                {
                    throw new Exception("User not found");
                }

                if (!_passwordManager.VerifyPasswordHash(query.Password, userExist.PasswordHash, userExist.PasswordSalt))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
