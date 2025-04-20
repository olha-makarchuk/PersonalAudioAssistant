namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IBlobStorage
    {
        Task<bool> FileExistsAsync(string fileName);
        Task PutContextAsync(string filename, Stream file);
        List<int> FindByMessage(Guid messageId);
        Task<List<string>> GetAllAsync();
        Task DeleteAsync(string filename);
    }
}
