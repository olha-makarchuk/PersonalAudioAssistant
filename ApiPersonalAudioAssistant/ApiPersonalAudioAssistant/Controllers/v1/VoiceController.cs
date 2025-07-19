using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class VoiceController : BaseApiController
    {
        public VoiceController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> CreateVoice(CreateVoiceCommand command)
        {
            return Ok(await Mediator.Send(command));//string
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteVoice(DeleteVoiceCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVoice(UpdateVoiceCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("allvoices")]
        public async Task<IActionResult> GetAllVoicesByUserId(GetAllVoicesByUserIdQuery command)
        {
            var voicesList = await Mediator.Send(command);
            return Ok(voicesList);
        }

        [HttpPost("byid")]
        public async Task<IActionResult> GetVoiceById(GetVoiceByIdQuery command)
        {
            var voice = await Mediator.Send(command);
            return Ok(voice);
        }
    }
}
