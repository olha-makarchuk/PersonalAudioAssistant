using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MessageCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.PaymentCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.MessageQuery;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class PaymentController : BaseApiController
    {
        public PaymentController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(CreatePaymentCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePayment(UpdatePaymentCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("byid")]
        public async Task<IActionResult> GetPaymentByUserId(GetPaymentByUserIdQuery command)
        {
            var payment = await Mediator.Send(command);
            return Ok(payment);
        }
    }
}
