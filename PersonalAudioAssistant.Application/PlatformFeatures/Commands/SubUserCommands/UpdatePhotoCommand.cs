using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using System;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands
{
    public class UpdatePhotoCommand : IRequest
    {
        public string PhotoURL { get; set; }
        public string PhotoPath { get; set; }
    }

    public class UpdatePhotoCommandHandler : IRequestHandler<UpdatePhotoCommand>
    {
        private readonly ISubUserRepository _subUserRepository;
        private readonly IBlobStorage _blobStorage;

        public UpdatePhotoCommandHandler(ISubUserRepository subUserRepository, IBlobStorage blobStorage)
        {
            _subUserRepository = subUserRepository;
            _blobStorage = blobStorage;
        }

        public async Task Handle(UpdatePhotoCommand request, CancellationToken cancellationToken = default)
        {
            if (request.PhotoPath != null && request.PhotoURL != null)
            {
                using var stream = System.IO.File.OpenRead(request.PhotoPath);
                string fileName = $"{Path.GetFileName(request.PhotoURL)}?nocache=1";
                var a = await _blobStorage.FileExistsAsync(fileName, BlobContainerType.UserImage);

                await _blobStorage.DeleteAsync(fileName, BlobContainerType.UserImage);
                //a = await _blobStorage.FileExistsAsync(fileName, BlobContainerType.UserImage);

                await _blobStorage.PutContextAsync(fileName, stream, BlobContainerType.UserImage);
            }
        }
    }
}
