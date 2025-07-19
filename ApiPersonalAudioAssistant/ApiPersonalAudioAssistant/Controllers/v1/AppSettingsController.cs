using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SettingsCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.SettingsQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AppSettingsController : BaseApiController
    {
        public AppSettingsController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAppSettingsCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateSettingsCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("balance")]
        public async Task<IActionResult> UpdateBalance(UpdateBalanceCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("getsettings")]
        public async Task<IActionResult> GetSettingsByUserId(GetSettingsByUserIdQuery command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
