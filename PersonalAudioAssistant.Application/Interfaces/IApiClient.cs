namespace PersonalAudioAssistant.Application.Interfaces
{
    public interface IApiClient
    {
        Task<List<double>> CreateVoiceEmbedding(Stream audioStream);
    }
}
