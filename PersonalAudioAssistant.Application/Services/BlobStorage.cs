using PersonalAudioAssistant.Application.Interfaces;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PersonalAudioAssistant.Application.Services
{
    public class BlobStorage : IBlobStorage
    {
        private readonly BlobServiceClient _client;

        public BlobStorage(BlobStorageConfig config)
        {
            _client = new BlobServiceClient(config.ConnectionString);
        }

        public async Task<bool> FileExistsAsync(string fileName, BlobContainerType containerType)
        {
            var blobClient = GetContainer(containerType).GetBlobClient(fileName);
            var exists = await blobClient.ExistsAsync();
            if (!exists)
            {
                throw new Exception($"Blob '{fileName}' does not exist in container '{containerType}'.");
            }
            return exists;
        }

        public async Task PutContextAsync(string filename, Stream stream, BlobContainerType containerType)
        {
            try
            {
                var blobClient = GetContainer(containerType).GetBlobClient(filename);

                var headers = new BlobHttpHeaders
                {
                    ContentType = "audio/mpeg"
                };

                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = headers
                });
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error uploading blob '{filename}' to container '{containerType}': {ex.Message}");
            }
        }



        public async Task PutContextAsync2(string filenamelocal, BlobContainerType containerType)
        {
            try
            {
                string fileName = Path.GetFileName(filenamelocal);
                var blobClient = GetContainer(containerType).GetBlobClient(fileName);

                byte[] buffer = await File.ReadAllBytesAsync(filenamelocal);
                BinaryData binaryData = new BinaryData(buffer);

                // Set the content type to audio/mpeg
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "audio/mpeg"
                };

                // Upload the file with the content type set
                await blobClient.UploadAsync(binaryData, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error uploading blob to container '{containerType}': {ex.Message}");
            }
        }


        public List<int> FindByMessage(Guid messageId, BlobContainerType containerType)
        {
            var results = GetContainer(containerType)
                .GetBlobs(prefix: messageId.ToString("N"))
                .AsPages(default, 1000)
                .SelectMany(dt => dt.Values)
                .Select(bi => int.Parse(bi.Name.Split('_').Last()))
                .ToList();

            return results;
        }

        public async Task<List<string>> GetAllAsync(BlobContainerType containerType)
        {
            var results = GetContainer(containerType)
                .GetBlobs()
                .Select(blobItem => blobItem.Name)
                .ToList();

            return results;
        }

        public async Task DeleteAsync(string filename, BlobContainerType containerType)
        {
            var blobClient = GetContainer(containerType).GetBlobClient(filename);

            try
            {
                var exists = await blobClient.ExistsAsync();
                if (exists)
                {
                    await blobClient.DeleteAsync();
                }
                else
                {
                    throw new Exception($"Blob '{filename}' does not exist in container '{containerType}'.");
                }
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error deleting blob '{filename}' from container '{containerType}': {ex.Message}");
            }
        }

        private BlobContainerClient GetContainer(BlobContainerType type)
        {
            return type switch
            {
                BlobContainerType.AudioMessage => _client.GetBlobContainerClient("audio-message"),
                BlobContainerType.UserImage => _client.GetBlobContainerClient("user-image"),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported container type: {type}")
            };
        }

        public async Task PutContextAsyncBytes(string filename, byte[] bytes, BlobContainerType containerType)
        {

        }
    }

    public enum BlobContainerType
    {
        AudioMessage,
        UserImage
    }
}
