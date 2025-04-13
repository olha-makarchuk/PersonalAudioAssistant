using System.Globalization;
using Android.Content;
using Android.Speech;
using CommunityToolkit.Maui.Alerts;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Services;
using Plugin.Maui.Audio;

namespace PersonalAudioAssistant.Platforms
{
    public sealed class SpeechToTextImplementation : ISpeechToText
    {
        private SpeechRecognitionListener? listener;
        private SpeechRecognizer? speechRecognizer;
        private string wsUrl = "ws://10.0.2.2:8000/ws/audio";

        public async Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, List<SubUser> listUsers, CancellationToken cancellationToken)
        {
            var taskResult = new TaskCompletionSource<string>();

            listener = new SpeechRecognitionListener
            {
                Error = ex => taskResult.TrySetException(new Exception("Failure in speech engine - " + ex)),

                PartialResults = async sentence =>
                {
                    bool processingCommand = false;
                    recognitionResult?.Report(sentence);

                    SubUser? matchedUser = null;
                    string normalizedSentence = sentence.Trim().ToLowerInvariant();
                    matchedUser = listUsers.FirstOrDefault(user =>
                        !string.IsNullOrWhiteSpace(user.StartPhrase) &&
                        normalizedSentence.Contains(user.StartPhrase.Trim().ToLowerInvariant())
                    );

                    if (matchedUser != null && !processingCommand)
                    {
                        processingCommand = true;

                        try
                        {
                            PauseListening();

                            await Toast.Make(matchedUser.StartPhrase).Show(cancellationToken);

                            var audioPlayerHelper = new AudioPlayerHelper(new AudioManager());
                            await audioPlayerHelper.PlayAudio();

                            IAudioDataProvider audioProvider = new AndroidAudioDataProvider();
                            var transcriber = new ApiClientAudio(wsUrl, audioProvider);

                            string answer = await transcriber.StreamAudioDataAsync(matchedUser, cancellationToken);
                            await Toast.Make($"Відповідь: {answer}").Show(cancellationToken);

                            await Task.Delay(100);
                        }
                        catch (Exception ex)
                        {
                            await Toast.Make("Помилка з listener: " + ex.Message).Show(cancellationToken);
                        }
                        finally
                        {
                            processingCommand = false;
                            RestartListening(culture);
                        }
                    }
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

        private void RestartListening(CultureInfo culture)
        {
            if (speechRecognizer == null)
            {
                speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
                speechRecognizer.SetRecognitionListener(listener);
            }
            speechRecognizer.StartListening(CreateSpeechIntent(culture));
        }

        public ValueTask DisposeAsync()
        {
            listener?.Dispose();
            speechRecognizer?.Dispose();
            return ValueTask.CompletedTask;
        }

        private void PauseListening()
        {
            speechRecognizer?.StopListening();
        }

        private void StopRecording()
        {
            speechRecognizer?.StopListening();
            speechRecognizer?.Destroy();
        }
    }
}
