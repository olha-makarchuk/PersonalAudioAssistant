using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MainUserCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.MainUserQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class MainUserController : BaseApiController
    {
        public MainUserController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("byemail")]
        public async Task<IActionResult> GetMainUserByEmail(GetMainUserByEmailQuery command)
        {
            var mainUser = await Mediator.Send(command);
            return Ok(mainUser);
        }

        [HttpPost("update-mainuser")]
        public async Task<IActionResult> UpdateMainUser(UpdateMainUserCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMainUser(CreateMainUserCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
