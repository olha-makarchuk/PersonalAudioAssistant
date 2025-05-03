using System.Globalization;
using Android.Content;
using Android.Speech;
using CommunityToolkit.Maui.Alerts;
using MediatR;
using Newtonsoft.Json;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
using PersonalAudioAssistant.ViewModel;
using Plugin.Maui.Audio;

namespace PersonalAudioAssistant.Platforms
{
    public sealed class SpeechToTextImplementation : ISpeechToText
    {
        private SpeechRecognitionListener? listener;
        private SpeechRecognizer? speechRecognizer;
        private bool isListening = false;
        private bool IsContinueConversation;
        string _prevResponseId = null;
        private bool IsFirstRequest; 
        private string PreviousResponseId;
        private IMediator _mediatr;
        private Action? _clearChatMessagesAction;
        private readonly ConversationApiClient _conversationApiClient;
        private readonly MessagesApiClient _messagesApiClient;
        private readonly MoneyUsedApiClient _moneyUsedApiClient;
        private readonly MoneyUsersUsedApiClient _moneyUsersUsedApiClient;
        private readonly VoiceApiClient _voiceApiClient;
        private readonly ManageCacheData _manageCacheData;

        public bool IsPrivateConversation { get; set; }

        public SpeechToTextImplementation(IMediator mediatr, ConversationApiClient conversationApiClient, MessagesApiClient messagesApiClient, ManageCacheData manageCacheData, MoneyUsedApiClient moneyUsedApiClient, MoneyUsersUsedApiClient moneyUsersUsedApiClient, VoiceApiClient voiceApiClient)
        {
            _mediatr = mediatr;
            _conversationApiClient = conversationApiClient;
            _messagesApiClient = messagesApiClient;
            _manageCacheData = manageCacheData;
            _moneyUsedApiClient = moneyUsedApiClient;
            _moneyUsersUsedApiClient = moneyUsersUsedApiClient;
            _voiceApiClient = voiceApiClient;
        }
        public SpeechToTextImplementation() : this(MediatorProvider.Mediator,
                MediatorProvider.Services!.GetRequiredService<ConversationApiClient>(), MediatorProvider.Services!.GetRequiredService<MessagesApiClient>(), MediatorProvider.Services!.GetRequiredService<ManageCacheData>(), MediatorProvider.Services!.GetRequiredService<MoneyUsedApiClient>(), MediatorProvider.Services!.GetRequiredService<MoneyUsersUsedApiClient>(), MediatorProvider.Services!.GetRequiredService<VoiceApiClient>())
        {
        }

        public async Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, IProgress<ChatMessage> chatMessageProgress, List<SubUserResponse> listUsers, CancellationToken cancellationToken, Action clearChatMessagesAction)
        {
            var taskResult = new TaskCompletionSource<string>();
            _clearChatMessagesAction = clearChatMessagesAction;

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

                PartialResults = async sentence =>
                {
                    bool processingCommand = false;
                    recognitionResult?.Report(sentence);
                    var apiGPT = new ApiClientGPT();

                    SubUserResponse? matchedUser = null;
                    string normalizedSentence = sentence.Trim().ToLowerInvariant();

                    bool isPrivateConversation = normalizedSentence.Contains("особиста розмова");

                    matchedUser = listUsers.FirstOrDefault(user =>
                        !string.IsNullOrWhiteSpace(user.startPhrase) &&
                        normalizedSentence.Contains(user.startPhrase.Trim().ToLowerInvariant())
                    );
                    var convMain = await _manageCacheData.GetСonversationAsync();
                    //string convId = convMain[0].IdConversation;

                    if (matchedUser != null && !processingCommand)
                    {
                        processingCommand = true;

                        try
                        {
                            PauseListening();

                            await Toast.Make(matchedUser.startPhrase).Show(cancellationToken);

                            var audioPlayerHelper = new AudioPlayerHelper(new AudioManager());
                            await audioPlayerHelper.PlayAudio(cancellationToken);

                            IsContinueConversation = true;
                            IsFirstRequest = true;

                            var conversationTask = _conversationApiClient.CreateConversationAsync("", matchedUser.id);

                            while (IsContinueConversation)
                            {
                                try
                                {
                                    var voice = await _voiceApiClient.GetVoiceByIdAsync(matchedUser.voiceId);
                                    IAudioDataProvider audioProvider = new AndroidAudioDataProvider();
                                    var transcriber = new ApiClientAudio(audioProvider, new WebSocketService());

                                    var transcription = await transcriber.StreamAudioDataAsync(matchedUser, cancellationToken, IsFirstRequest, PreviousResponseId);

                                    TranscriptionResponse response = JsonConvert.DeserializeObject<TranscriptionResponse>(transcription.Response);

                                    if (response.Request == "none")
                                    {
                                        IsContinueConversation = false;
                                        return;
                                    }

                                    await Toast.Make($"Відповідь: {transcription.Response}").Show(cancellationToken);

                                    ApiClientGptResponse answer = await apiGPT.ContinueChatAsync(transcription.Response, _prevResponseId);
                                    _prevResponseId = answer.responseId;

                                    var createUserCmd = new CreateMessageCommand
                                    {
                                        ConversationId = conversationTask.Result,
                                        Text = response.Request,
                                        UserRole = "user",
                                        Audio = transcription.Audio,
                                        LastRequestId = _prevResponseId,
                                        SubUserId = matchedUser.id
                                    };
                                    var createdUser = await _messagesApiClient.CreateMessageAsync(createUserCmd);
                                    await CalculatePrice(response.AudioDuration, answer, matchedUser.id, matchedUser.userId);

                                    chatMessageProgress.Report(new ChatMessage
                                    {
                                        Text = createdUser.text,
                                        UserRole = "user",
                                        DateTimeCreated = createdUser.dateTimeCreated,
                                        URL = createdUser.audioPath
                                    });

                                    IsContinueConversation = response.IsContinuous;
                                    IsFirstRequest = false;
                                    if(!IsContinueConversation)
                                    {
                                        _prevResponseId = null;
                                      //clearChatMessagesAction?.Invoke();
                                    }

                                    var textToSpeech = new ElevenlabsApi();
                                    var audioBytes = await textToSpeech.ConvertTextToSpeechAsync(voice.voiceId, answer.text);

                                    //await Task.WhenAll(userTask);

                                    var careteMessageAI = new CreateMessageCommand()
                                    {
                                        ConversationId = conversationTask.Result,
                                        Text = answer.text,
                                        UserRole = "ai",
                                        Audio = audioBytes,
                                        LastRequestId = _prevResponseId,
                                        SubUserId = matchedUser.id
                                    };
                                    var createdAI = await _messagesApiClient.CreateMessageAsync(careteMessageAI);

                                    chatMessageProgress.Report(new ChatMessage
                                    {
                                        Text = createdAI.text,
                                        UserRole = "ai",
                                        DateTimeCreated = createdAI.dateTimeCreated,
                                        URL = createdAI.audioPath
                                    });

                                    await audioPlayerHelper.PlayAudioFromBytesAsync(audioBytes, cancellationToken);
                                    //await Task.WhenAll(aiTask);
                                    await Task.Delay(100);
                                }
                                catch(Exception ex)
                                {
                                    StopRecording();
                                    return;
                                }
                            }
                            ApiClientGptResponse description = await apiGPT.ContinueChatAsync("На основі розмови напиши короткий заголовок, який підсумовує основну тему", _prevResponseId);

                            var TaskDescription =  _conversationApiClient.UpdateConversationAsync(conversationTask.Result, description.text);
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
                //clearChatMessagesAction?.Invoke();
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

        public async Task CalculatePrice(double audioDurationRequest, ApiClientGptResponse gptResponse, string subUserId, string mainUserId)
        {
            var transcriptionCost = (audioDurationRequest / 60.0) * 0.006;

            var gptInCost = gptResponse.inputTokens * 0.000002;
            var gptOutCost = gptResponse.outputTokens * 0.000008;

            var charCount = gptResponse.text.Length;
            var ttsCost = (charCount / 1000.0) * 0.0833;

            double totalCost = transcriptionCost + gptInCost + gptOutCost + ttsCost;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _moneyUsedApiClient.CreateMoneyUsedAsync(mainUserId, subUserId, totalCost);
                }
                catch (Exception ex)
                {
                }
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    await _moneyUsersUsedApiClient.CreateMoneyUsersUsedAsync(subUserId, totalCost);
                }
                catch (Exception ex)
                {
                }
            });
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

            //_clearChatMessagesAction?.Invoke();
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

    public static class MediatorProvider
    {
        public static IServiceProvider? Services { get; set; }

        public static IMediator Mediator =>
            Services!.GetRequiredService<IMediator>();
    }

    public class TranscriptionResponse
    {
        public string Request { get; set; }
        public double AudioDuration { get; set; }
        public bool IsContinuous { get; set; }
    }
}
