using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MoneyUsedCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.MoneyUsedQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class MoneyUsedController : BaseApiController
    {
        public MoneyUsedController(IMediator mediator) : base(mediator) { }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMoneyUsed(CreateMoneyUsedCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("get")]
        public async Task<IActionResult> GetMoneyUsed(GetMoneyUsedByMainUserIdQuery command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
