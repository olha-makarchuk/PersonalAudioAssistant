using Android.Media;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Services
{
    public class AndroidAudioDataProvider : IAudioDataProvider, IDisposable
    {
        private const int RATE = 16000;
        private const ChannelIn CHANNEL_CONFIG = ChannelIn.Mono;
        private const Encoding AUDIO_FORMAT = Encoding.Pcm16bit;
        private readonly int minBufferSize;
        private AudioRecord audioRecord;
        private bool disposed;

        public AndroidAudioDataProvider()
        {
            minBufferSize = AudioRecord.GetMinBufferSize(RATE, CHANNEL_CONFIG, AUDIO_FORMAT);
            audioRecord = new AudioRecord(
                AudioSource.Mic,
                RATE,
                CHANNEL_CONFIG,
                AUDIO_FORMAT,
                minBufferSize);
            audioRecord.StartRecording();
        }

        public Task<byte[]> GetAudioDataAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[minBufferSize];
            int bytesRead = audioRecord.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                if (bytesRead < buffer.Length)
                {
                    byte[] result = new byte[bytesRead];
                    Array.Copy(buffer, result, bytesRead);
                    return Task.FromResult(result);
                }
                return Task.FromResult(buffer);
            }
            return Task.FromResult(new byte[0]);
        }

        public void StopRecording()
        {
            if (audioRecord?.RecordingState == RecordState.Recording)
            {
                audioRecord.Stop();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                StopRecording();
                audioRecord?.Release();
                audioRecord = null;
                disposed = true;
            }
        }
    }
}
