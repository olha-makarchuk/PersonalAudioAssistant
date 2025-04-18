using System.Globalization;
using Android.Content;
using Android.Speech;
using CommunityToolkit.Maui.Alerts;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;
using Plugin.Maui.Audio;

namespace PersonalAudioAssistant.Platforms
{
    public sealed class SpeechToTextImplementation : ISpeechToText
    {
        private SpeechRecognitionListener? listener;
        private SpeechRecognizer? speechRecognizer;
        private bool isListening = false;

        public async Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, List<SubUserResponse> listUsers, CancellationToken cancellationToken)
        {
            var taskResult = new TaskCompletionSource<string>();

            listener = new SpeechRecognitionListener
            {
                Error = async ex =>
                {
                    try
                    {
                        await Toast.Make("Помилка: " + ex).Show(cancellationToken);
                        PauseListening();
                        if (culture != null)
                        {
                            RestartListening(culture);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        await Toast.Make("Не вдалося перезапустити: " + innerEx.Message).Show(cancellationToken);
                    }

                    taskResult.TrySetException(new Exception("Failure in speech engine - " + ex));
                },

                //Error = ex => taskResult.TrySetException(new Exception("Failure in speech engine - " + ex)),

                PartialResults = async sentence =>
                {
                    bool processingCommand = false;
                    recognitionResult?.Report(sentence);

                    SubUserResponse? matchedUser = null;
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
                            await audioPlayerHelper.PlayAudio(cancellationToken);

                            IAudioDataProvider audioProvider = new AndroidAudioDataProvider();
                            var transcriber = new ApiClientAudio(audioProvider, new WebSocketService());

                            try
                            {
                                string answer = await transcriber.StreamAudioDataAsync(matchedUser, cancellationToken);

                                await Toast.Make($"Відповідь: {answer}").Show(cancellationToken);

                                var textToSpeech = new ElevenlabsApi();
                                var audioBytes = await textToSpeech.ConvertTextToSpeechAsync(matchedUser.VoiceId!, answer);

                                await audioPlayerHelper.PlayAudioFromBytesAsync(audioBytes, cancellationToken);

                                await Task.Delay(100);
                            }
                            catch
                            {
                                StopRecording();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            StopRecording();
                            await Toast.Make("Помилка з listener: " + ex.Message).Show(cancellationToken);
                            return; 
                        }

                        await Task.Delay(1000);
                        RestartListening(culture);
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
            StartListening(culture);

            await using (cancellationToken.Register(() =>
            {
                StopRecording();
                taskResult.TrySetResult(string.Empty); 
            }))
            {
                try
                {
                    return await taskResult.Task;
                }
                catch (OperationCanceledException)
                {
                    StopRecording();
                    await Toast.Make("Розпізнавання зупинено").Show(cancellationToken);
                    return string.Empty;
                }
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

        public ValueTask DisposeAsync()
        {
            listener?.Dispose();
            speechRecognizer?.Dispose();
            return ValueTask.CompletedTask;
        }

        private void PauseListening()
        {
            if (isListening)
            {
                speechRecognizer?.StopListening();
                isListening = false;
            }
        }

        private void StopRecording()
        {
            if (isListening)
            {
                speechRecognizer?.StopListening();
                isListening = false;
            }

            speechRecognizer?.Destroy();
            speechRecognizer = null;
        }
        private void StartListening(CultureInfo culture)
        {
            if (speechRecognizer == null)
            {
                speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
                speechRecognizer.SetRecognitionListener(listener);
            }

            if (!isListening)
            {
                isListening = true;
                speechRecognizer.StartListening(CreateSpeechIntent(culture));
            }
        }
        private void RestartListening(CultureInfo culture)
        {
            StopRecording();
            StartListening(culture);
        }
    }
}
