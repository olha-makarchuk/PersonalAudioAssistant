using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.AutoPaymentsQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AutoPaymentController : BaseApiController
    {
        public AutoPaymentController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAutoPaymentCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateAutoPaymentCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("byuserid")]
        public async Task<IActionResult> GetAutoPaymentsByUserId(GetAutoPaymentsByUserIdQuery command)
        {
            var autoPayment = await Mediator.Send(command);

            return Ok(autoPayment);
        }
    }
}
