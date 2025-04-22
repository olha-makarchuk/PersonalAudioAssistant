using PersonalAudioAssistant.Application.Services;

namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IBlobStorage
    {
        Task<bool> FileExistsAsync(string fileName, BlobContainerType containerType);
        Task PutContextAsync(string filename, Stream file, BlobContainerType containerType);
        List<int> FindByMessage(Guid messageId, BlobContainerType containerType);
        Task<List<string>> GetAllAsync(BlobContainerType containerType);
        Task DeleteAsync(string filename, BlobContainerType containerType);
    }
}
