using ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using ApiPersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using ApiPersonalAudioAssistant.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiPersonalAudioAssistant.Controllers.v1
{
    [ApiVersion("1.0")]
    public class SubUserController : BaseApiController
    {
        public SubUserController(IMediator mediator) : base(mediator) { }

        [HttpPost("create")]
        public async Task<IActionResult> AddSubUser([FromForm] IFormFile file, [FromForm] string command)
        {
            var deserializedCommand = JsonSerializer.Deserialize<AddSubUserCommand>(command, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (deserializedCommand == null)
                return BadRequest("Invalid command data.");

            deserializedCommand.Photo = file;

            return Ok(await Mediator.Send(deserializedCommand));
        }

        [HttpDelete("password")]
        public async Task<IActionResult> DeletePasswordSubUser(DeletePasswordSubUserCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSubUser(DeleteSubUserCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut("photo")]
        public async Task<IActionResult> UpdatePhoto(UpdatePhotoCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("change-photo")]
        public async Task<IActionResult> ChangePhoto([FromForm] UpdatePhotoCommand command)
        {
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPost("update-user-voice")]
        public async Task<IActionResult> UpdateUserVoice(UpdateUserVoiceCommand command)
        {
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPost("update-voice")]
        public async Task<IActionResult> UpdateVoice(UpdateVoiceActingCommand command)
        {
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPost("update-personal-information")]
        public async Task<IActionResult> UpdatePersonalInformation(UpdatePersonalInformationCommand command)
        {
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePasswordInformation(UpdatePasswordCommand command)
        {
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubUser(UpdateSubUserCommand command)
        {
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPost("check-password")]
        public async Task<IActionResult> CheckSubUserPassword(CheckSubUserPasswordQuery command)
        {
            return Ok(await Mediator.Send(command)); // bool
        }

        [HttpPost("allusers")]
        public async Task<IActionResult> GetAllUsersByUserId(GetAllUsersByUserIdQuery command)
        {
            var subUsersList = await Mediator.Send(command);
            return Ok(subUsersList); 
        }

        [HttpPost("userbyid")]
        public async Task<IActionResult> GetUserById(GetUserByIdQuery command)
        {
            var subUser = await Mediator.Send(command);
            return Ok(subUser);
        }

        [HttpPost("userbystartphrase")]
        public async Task<IActionResult> GetUserByStartPhrase(GetUserByStartPhraseQuery command)
        {
            var subUser = await Mediator.Send(command);
            return Ok(subUser);
        }
    }
}
