using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MoneyUsersUsedCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.MoneyUsersUsedQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class MoneyUsersUsedController : BaseApiController
    {
        public MoneyUsersUsedController(IMediator mediator) : base(mediator) { }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMoneyUsersUsed(CreateMoneyUsersUsedCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("get")]
        public async Task<IActionResult> GetMoneyUsersUsed(GetMoneyUsersUsedByMainUserIdQuery command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
