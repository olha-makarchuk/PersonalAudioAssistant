using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Application.Services;
using Microsoft.AspNetCore.Http;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdatePhotoCommand : IRequest<Unit>
    {
        public string PhotoURL { get; set; }
        public IFormFile Photo { get; set; }
    }

    public class UpdatePhotoCommandHandler : IRequestHandler<UpdatePhotoCommand, Unit>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IBlobStorage _blobStorage;

        public UpdatePhotoCommandHandler(ISubUserRepository subUserRepository, IBlobStorage blobStorage)
        {
            _subUserRepository = subUserRepository;
            _blobStorage = blobStorage;
        }

        public async Task<Unit> Handle(UpdatePhotoCommand request, CancellationToken cancellationToken = default)
        {
            if (request.Photo != null && request.PhotoURL != null)
            {
                using var stream = request.Photo.OpenReadStream();
                string fileName = $"{Path.GetFileName(request.PhotoURL)}";

                var exists = await _blobStorage.FileExistsAsync(fileName, BlobContainerType.UserImage);
                if (exists)
                {
                    await _blobStorage.DeleteAsync(fileName, BlobContainerType.UserImage);
                }

                await _blobStorage.PutContextAsync(fileName, stream, BlobContainerType.UserImage);
            }
            return Unit.Value;
        }
    }
}
