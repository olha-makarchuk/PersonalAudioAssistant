namespace ApiPersonalAudioAssistant.Application.Services
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
