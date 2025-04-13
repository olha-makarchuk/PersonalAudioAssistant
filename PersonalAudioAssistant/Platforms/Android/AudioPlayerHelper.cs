using Plugin.Maui.Audio;

namespace PersonalAudioAssistant.Platforms
{
    public class AudioPlayerHelper
    {
        readonly IAudioManager audioManager;

        public AudioPlayerHelper(IAudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public async Task PlayAudio()
        {
            var audioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("Resources/Media/Ask.wav"));

            var tcs = new TaskCompletionSource();
            audioPlayer.PlaybackEnded += (sender, args) => tcs.TrySetResult();

            audioPlayer.Play();
            await tcs.Task;
        }
    }
}
