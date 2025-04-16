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

        // Метод для відтворення аудіо з вбудованого ресурсу (залишаємо)
        public async Task PlayAudio(CancellationToken cancellationToken)
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
        }

        public async Task PlayAudioFromBytesAsync(byte[] audioData, CancellationToken cancellationToken)
        {
            // Створюємо тимчасовий шлях до файлу
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, $"temp_audio_{Guid.NewGuid()}.mp3");

            // Записуємо байти в тимчасовий файл
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
                    // Завжди очищаємо тимчасовий файл
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
