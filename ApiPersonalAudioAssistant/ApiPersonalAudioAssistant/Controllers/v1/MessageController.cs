using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.MessageCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.MessageQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class MessageController : BaseApiController
    {
        public MessageController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessagesByConversationId(DeleteMessagesByConversationIdCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("byconversationid")]
        public async Task<IActionResult> GetConversationById(GetMessagesByConversationIdQuery command)
        {
            var messages = await Mediator.Send(command);
            return Ok(messages);
        }
    }
}
