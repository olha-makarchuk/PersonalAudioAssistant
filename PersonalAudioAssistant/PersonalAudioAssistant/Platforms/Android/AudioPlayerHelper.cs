using Plugin.Maui.Audio;
using System.IO;
using System.Threading;

namespace PersonalAudioAssistant.Platforms
{
    public class AudioPlayerHelper
    {
        private readonly IAudioManager audioManager;

        public AudioPlayerHelper(IAudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public async Task PlayAudioFromUrlAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                var audioBytes = await httpClient.GetByteArrayAsync(url, cancellationToken);
                await PlayAudioFromBytesAsync(audioBytes, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing audio from URL: {ex.Message}");
            }
        }

        public async Task PlayAudio(CancellationToken cancellationToken)
        {
            try
            {
                var audioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("Resources/Media/Ask.wav"));
                var tcs = new TaskCompletionSource();
                audioPlayer.PlaybackEnded += (sender, args) => tcs.TrySetResult();
                using (cancellationToken.Register(() =>
                {
                    if (audioPlayer.IsPlaying)
                    {
                        audioPlayer.Stop();
                    }
                    tcs.TrySetCanceled();
                }))
                {
                    audioPlayer.Play();
                    await tcs.Task;
                }
                audioPlayer.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing audio: {ex.Message}");
            }
        }


        public async Task PlayAudioFromBytesAsync(byte[] audioData, CancellationToken cancellationToken)
        {
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, $"temp_audio_{Guid.NewGuid()}.mp3");

            await File.WriteAllBytesAsync(tempFilePath, audioData, cancellationToken);

            var stream = File.OpenRead(tempFilePath);
            var audioPlayer = audioManager.CreatePlayer(stream);

            var tcs = new TaskCompletionSource();

            audioPlayer.PlaybackEnded += (sender, args) => tcs.TrySetResult();

            using (cancellationToken.Register(() =>
            {
                if (audioPlayer.IsPlaying)
                {
                    audioPlayer.Stop();
                }
                tcs.TrySetCanceled();
            }))
            {
                audioPlayer.Play();
                try
                {
                    await tcs.Task;
                }
                finally
                {
                    audioPlayer.Dispose();
                    stream.Dispose();

                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
            }
        }
    }
}
