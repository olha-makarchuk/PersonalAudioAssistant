using PersonalAudioAssistant.Application.Services;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IBlobStorage
    {
        Task<bool> FileExistsAsync(string fileName, BlobContainerType containerType);
        Task PutContextAsync(string filename, Stream stream, BlobContainerType containerType);
        Task PutContextAsyncBytes(string filename, byte[] bytes, BlobContainerType containerType);
        List<int> FindByMessage(Guid messageId, BlobContainerType containerType);
        Task<List<string>> GetAllAsync(BlobContainerType containerType);
        Task DeleteAsync(string filename, BlobContainerType containerType);
        Task PutContextAsync2(string filename, BlobContainerType containerType);
    }
}
