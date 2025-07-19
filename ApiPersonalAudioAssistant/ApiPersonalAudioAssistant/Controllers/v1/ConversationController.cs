using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.AutoPaymentsCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.ConversationCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.ConversationQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class ConversationController : BaseApiController
    {
        public ConversationController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> Create(CreateConversationCommand command)
        {
            return Ok(await Mediator.Send(command));//string Id
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateConversationCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteConversationById(DeleteConversationCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("byid")]
        public async Task<IActionResult> GetConversationById(GetConversationByIdQuery command)
        {
            var conversation = await Mediator.Send(command);
            return Ok(conversation);
        }

        [HttpPost("bysubuser")]
        public async Task<IActionResult> GetConversationsBySubUserId(GetConversationsBySubUserIdQuery command)
        {
            var conversationsList = await Mediator.Send(command);
            return Ok(conversationsList);
        }
    }
}
