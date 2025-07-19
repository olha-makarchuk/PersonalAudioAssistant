using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.PaymentHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class PaymentHistoryController : BaseApiController
    {
        public PaymentHistoryController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> GetPaymentHistoryByUserId(GetPaymentHistoryByUserIdQuery command)
        {
            var paymentHistoryList = await Mediator.Send(command);
            return Ok(paymentHistoryList);
        }
    }
}
