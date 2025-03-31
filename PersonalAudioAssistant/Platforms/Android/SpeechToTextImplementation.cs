using System.Globalization;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using CommunityToolkit.Maui.Alerts;
using Plugin.Maui.Audio;

namespace PersonalAudioAssistant.Platforms
{
    public sealed class SpeechToTextImplementation : ISpeechToText
    {
        private SpeechRecognitionListener? listener;
        private SpeechRecognizer? speechRecognizer;
        readonly IAudioManager _audioManager;
        readonly IAudioRecorder _audioRecorder;
        private string wsUrl = "ws://10.0.2.2:8000/ws/audio";

        public SpeechToTextImplementation(IAudioManager audioManager)
        {
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
        }

        public SpeechToTextImplementation()
        {
        }

        public async Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, List<IndividualParameters> allParameters, CancellationToken cancellationToken)
        {
            var taskResult = new TaskCompletionSource<string>();

            listener = new SpeechRecognitionListener
            {
                Error = ex => taskResult.TrySetException(new Exception("Failure in speech engine - " + ex)),

                PartialResults = async sentence =>
                {
                    /*
                    bool processingCommand = false;

                    recognitionResult?.Report(sentence);

                    IndividualParameters? matchedPhrase = null;
                    string normalizedSentence = sentence.Trim().ToLowerInvariant();

                    foreach (var parameter in allParameters)
                    {
                        if (string.IsNullOrWhiteSpace(parameter.StartPhrase))
                            continue;

                        string normalizedPhrase = parameter.StartPhrase.Trim().ToLowerInvariant();
                        if (normalizedSentence.Contains(normalizedPhrase))
                        {
                            matchedPhrase = parameter;
                            break;
                        }
                    }
                    if (matchedPhrase != null)
                    {
                        if (processingCommand)
                            return;

                        processingCommand = true;
                        PauseListening();
                        await Toast.Make(matchedPhrase.StartPhrase).Show(cancellationToken);

                        AudioPlayerHelper audioPlayerViewModel = new AudioPlayerHelper(new AudioManager());
                        await audioPlayerViewModel.PlayAudio();

                        IAudioDataProvider audioProvider = new AndroidAudioDataProvider();
                        var transcriber = new AudioTranscriptionClient(wsUrl, audioProvider);

                        string answer = await transcriber.StreamAudioDataAsync(matchedPhrase.ApplicationUserId, matchedPhrase.VoiceId, cancellationToken);
                        await Toast.Make($"Відповідь: {answer}").Show(cancellationToken);
                        await Task.Delay(100);
                        RestartListening(culture);
                    }
                    processingCommand = false;*/
                },
                Results = sentence => taskResult.TrySetResult(sentence)
            };

            speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
            if (speechRecognizer is null)
            {
                throw new ArgumentException("Speech recognizer is not available");
            }

            speechRecognizer.SetRecognitionListener(listener);
            speechRecognizer.StartListening(CreateSpeechIntent(culture));

            await using (cancellationToken.Register(() =>
            {
                StopRecording();
                taskResult.TrySetCanceled();
            }))
            {
                return await taskResult.Task;
            }
        }

        private void PauseListening()
        {
            speechRecognizer?.StopListening();
        }

        private void RestartListening(CultureInfo culture)
        {
            // Якщо speechRecognizer був знищений, створюємо новий
            if (speechRecognizer == null)
            {
                speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
                speechRecognizer.SetRecognitionListener(listener);
            }
            speechRecognizer.StartListening(CreateSpeechIntent(culture));
        }

        private void StopRecording()
        {
            speechRecognizer?.StopListening();
            speechRecognizer?.Destroy();
        }

        private Intent CreateSpeechIntent(CultureInfo culture)
        {
            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, Java.Util.Locale.Default);
            var javaLocale = Java.Util.Locale.ForLanguageTag(culture.Name);
            intent.PutExtra(RecognizerIntent.ExtraLanguage, javaLocale);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            intent.PutExtra(RecognizerIntent.ExtraCallingPackage, Android.App.Application.Context.PackageName);
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 120000);
            return intent;
        }

        public async Task<bool> RequestPermissions()
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            var isAvailable = SpeechRecognizer.IsRecognitionAvailable(Android.App.Application.Context);
            return status == PermissionStatus.Granted && isAvailable;
        }

        public ValueTask DisposeAsync()
        {
            listener?.Dispose();
            speechRecognizer?.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    public class SpeechRecognitionListener : Java.Lang.Object, IRecognitionListener
    {
        public Action<SpeechRecognizerError>? Error { get; set; }
        public Action<string>? PartialResults { get; set; }
        public Action<string>? Results { get; set; }

        public void OnBeginningOfSpeech() { }

        public void OnBufferReceived(byte[]? buffer) { }

        public void OnEndOfSpeech() { }

        public void OnError([GeneratedEnum] SpeechRecognizerError error)
        {
            Error?.Invoke(error);
        }

        public void OnEvent(int eventType, Bundle? @params) { }

        public void OnPartialResults(Bundle? partialResults)
        {
            SendResults(partialResults, PartialResults);
        }

        public void OnReadyForSpeech(Bundle? @params) { }

        public void OnResults(Bundle? results)
        {
            SendResults(results, Results);
        }

        public void OnRmsChanged(float rmsdB) { }

        private void SendResults(Bundle? bundle, Action<string>? action)
        {
            var matches = bundle?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches == null || matches.Count == 0)
            {
                return;
            }
            action?.Invoke(matches.First());
        }
    }

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
            await tcs.Task; // Очікуємо завершення
        }
    }
}
