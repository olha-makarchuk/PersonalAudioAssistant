using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.MainUserCommands
{
    public class CreateMainUserCommand: IRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class CreateMainUserCommandHandler : IRequestHandler<CreateMainUserCommand>
    {
        private readonly IMainUserRepository _mainUserRepository;

        public CreateMainUserCommandHandler(IMainUserRepository mainUserRepository)
        {
            _mainUserRepository = mainUserRepository;
        }

        public async Task Handle(CreateMainUserCommand command, CancellationToken cancellationToken)
        {
            var user = new MainUser
            {
                Email = command.Email,
                PasswordHash = command.Password
            };

            user = await _mainUserRepository.CreateUser(user, cancellationToken);
        }
    }
}
