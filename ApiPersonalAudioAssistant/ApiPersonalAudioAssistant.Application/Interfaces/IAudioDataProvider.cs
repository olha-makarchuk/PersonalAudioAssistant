namespace ApiPersonalAudioAssistant.Application.Interfaces
{
    public interface IAudioDataProvider
    {
        Task<byte[]> GetAudioDataAsync(CancellationToken cancellationToken);
    }
}
