using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using MediatR;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Domain.Entities;
using System.IO;
using System.Text;

namespace PersonalAudioAssistant.Application.PlatformFeatures.Commands.MessageCommands
{
    public class CreateMessageCommand : IRequest
    {
        public string ConversationId { get; set; }
        public string Text { get; set; }
        public string UserRole { get; set; }
        public byte[] Audio { get; set; }
    }

    public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommand>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IBlobStorage _blobStorage;

        public CreateMessageCommandHandler(IMessageRepository messageRepository, IBlobStorage blobStorage)
        {
            _messageRepository = messageRepository;
            _blobStorage = blobStorage;
        }

        public async Task Handle(CreateMessageCommand request, CancellationToken cancellationToken = default)
        {
            var bytesAudio = ConvertPcmToWav(request.Audio);

            var message = new Message
            {
                ConversationId = request.ConversationId,
                Text = request.Text,
                UserRole = request.UserRole,
                DateTimeCreated = DateTime.UtcNow
            };

            if (request.Audio != null && request.Audio.Length > 0)
            {
                string fileName = $"{Guid.NewGuid()}.wav";

                using (var stream = new MemoryStream(bytesAudio))
                {
                    await _blobStorage.PutContextAsync(fileName, stream, BlobContainerType.AudioMessage);
                }

            }

            // Збереження повідомлення у репозиторій
            await _messageRepository.AddMessageAsync(message, cancellationToken);
        }

        public static byte[] ConvertPcmToWav(byte[] pcmData, int sampleRate = 44100, short bitsPerSample = 16, short channels = 1)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);
            int subchunk2Size = pcmData.Length;
            int chunkSize = 36 + subchunk2Size;

            // RIFF header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(chunkSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // Subchunk1Size for PCM
            writer.Write((short)1); // AudioFormat (1 = PCM)
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            // data subchunk
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(subchunk2Size);
            writer.Write(pcmData);

            return memoryStream.ToArray();
        }
    }
}
