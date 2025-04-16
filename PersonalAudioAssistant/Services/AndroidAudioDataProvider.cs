using Android.Media;
using PersonalAudioAssistant.Application.Interfaces;

namespace PersonalAudioAssistant.Services
{
    public class AndroidAudioDataProvider : IAudioDataProvider, IDisposable
    {
        private const int RATE = 16000;
        private const ChannelIn CHANNEL_CONFIG = ChannelIn.Mono;
        private const Encoding AUDIO_FORMAT = Encoding.Pcm16bit;
        private const int BUFFER_MULTIPLIER = 4; // Множник для збільшення розміру буфера
        private readonly int minBufferSize;
        private readonly int largeBufferSize;
        private AudioRecord audioRecord;
        private bool disposed;

        public AndroidAudioDataProvider()
        {
            // Отримуємо мінімальний розмір буфера
            minBufferSize = AudioRecord.GetMinBufferSize(RATE, CHANNEL_CONFIG, AUDIO_FORMAT);
            // Визначаємо великий розмір буфера
            largeBufferSize = minBufferSize * BUFFER_MULTIPLIER;

            audioRecord = new AudioRecord(
                AudioSource.Mic,
                RATE,
                CHANNEL_CONFIG,
                AUDIO_FORMAT,
                largeBufferSize);
            audioRecord.StartRecording();
        }

        public Task<byte[]> GetAudioDataAsync(CancellationToken cancellationToken)
        {
            // Використовуємо розширений буфер для зчитування даних
            byte[] buffer = new byte[largeBufferSize];
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
