﻿using System.Globalization;
using Android.Content;
using Android.Speech;
using CommunityToolkit.Maui.Alerts;
using MediatR;
using Newtonsoft.Json;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Model;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;
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
        private readonly PaymentHistoryApiClient _paymentHistoryApiClient;
        private readonly AppSettingsApiClient _appSettingsApiClient;
        private readonly MessagesApiClient _messagesApiClient;
        private readonly MoneyUsedApiClient _moneyUsedApiClient;
        private readonly MoneyUsersUsedApiClient _moneyUsersUsedApiClient;
        private readonly VoiceApiClient _voiceApiClient;
        private readonly ManageCacheData _manageCacheData;
        private ApiClientGPT _apiClientGPT;
        private CancellationTokenSource _globalCts = new();

        private bool _hasCleared = false;
        private bool _isCancelled = false;
        public bool IsPrivateConversation { get; set; }
        private bool _isUserCancelled = false;

        public SpeechToTextImplementation(IMediator mediatr, ConversationApiClient conversationApiClient, MessagesApiClient messagesApiClient, ManageCacheData manageCacheData, MoneyUsedApiClient moneyUsedApiClient, MoneyUsersUsedApiClient moneyUsersUsedApiClient, VoiceApiClient voiceApiClient, ApiClientGPT apiClientGPT, PaymentHistoryApiClient paymentHistoryApiClient, AppSettingsApiClient appSettingsApiClient)
        {
            _mediatr = mediatr;
            _conversationApiClient = conversationApiClient;
            _messagesApiClient = messagesApiClient;
            _manageCacheData = manageCacheData;
            _moneyUsedApiClient = moneyUsedApiClient;
            _moneyUsersUsedApiClient = moneyUsersUsedApiClient;
            _voiceApiClient = voiceApiClient;
            _apiClientGPT = apiClientGPT;
            _paymentHistoryApiClient = paymentHistoryApiClient;
            _appSettingsApiClient = appSettingsApiClient;
        }
        public SpeechToTextImplementation() : this(DataProvider.Mediator, DataProvider.ConversationApiClient
                , DataProvider.MessagesApiClient, DataProvider.ManageCacheData, DataProvider.MoneyUsedApiClient, DataProvider.MoneyUsersUsedApiClient, DataProvider.VoiceApiClient, DataProvider.ApiClientGPT, DataProvider.PaymentHistoryApiClient, DataProvider.AppSettingsApiClient)
        {
        }

        public void HardCancelAll()
        {
            try
            {
                _globalCts.Cancel();              
                _globalCts.Dispose();             
                _globalCts = new CancellationTokenSource();  
            }
            catch { }

            try
            {
                StopRecording();   
            }
            catch { }

            try
            {
                speechRecognizer?.Destroy();
                speechRecognizer = null;
            }
            catch { }

            isListening = false;
            _isCancelled = true;
        }


        public async Task<string> Listen(CultureInfo culture, IProgress<string>? recognitionResult, IProgress<ChatMessage> chatMessageProgress, List<SubUserResponse> listUsers, CancellationToken cancellationToken, Action clearChatMessagesAction, Func<Task> restoreChatMessagesAction, string prevResponseId)
        {
            _isUserCancelled = false;

            cancellationToken.Register(() => CancelListening());

                listener = new SpeechRecognitionListener
                {
                    Error = async ex =>
                    {
                        try
                        {
                            await Toast.Make("Помилка: " + ex).Show(cancellationToken);
                            PauseListening();
                            if (!_isUserCancelled && culture != null)
                            {
                                RestartListening(culture);
                            }

                        }
                        catch (Exception innerEx)
                        {
                            await Toast.Make("Не вдалося перезапустити: " + innerEx.Message).Show(cancellationToken);
                        }
                    },

                    PartialResults = async sentence =>
                    {
                        bool processingCommand = false;
                        string conversationId = null;
                        recognitionResult?.Report(sentence);
                        SubUserResponse? matchedUser = null;
                        bool isPrivateConversation = false;
                        var textToSpeech = new ElevenlabsApi();

                        string normalizedSentence = sentence.Trim().ToLowerInvariant();
                        isPrivateConversation = normalizedSentence.Contains("особиста розмова");

                        if (isPrivateConversation && !_hasCleared)
                        {
                            clearChatMessagesAction?.Invoke();
                            _hasCleared = true;
                            IsPrivateConversation = true;
                        }
                        matchedUser = listUsers.FirstOrDefault(user =>
                            !string.IsNullOrWhiteSpace(user.startPhrase) &&
                            normalizedSentence.Contains(user.startPhrase.Trim().ToLowerInvariant())
                        );

                        if (matchedUser != null && !processingCommand)
                        {
                            processingCommand = true;

                            try
                            {
                                PauseListening();


                                var audioPlayerHelper = new AudioPlayerHelper(new AudioManager());
                                await audioPlayerHelper.PlayAudioFromUrlAsync($"https://audioassistantblob.blob.core.windows.net/first-message/{matchedUser.id}.wav", _globalCts.Token);

                                IsContinueConversation = true;
                                IsFirstRequest = true;
                                TranscriptionResponse response = new();

                                Task<string> conversationIdTask;
                                if (isPrivateConversation)
                                {
                                    conversationIdTask = _conversationApiClient.CreateConversationAsync("", matchedUser.id);
                                    _prevResponseId = null;
                                }
                                else
                                {
                                    conversationIdTask = _manageCacheData.GetConversationAsync();
                                    _prevResponseId = prevResponseId;
                                }

                                while (IsContinueConversation && !_globalCts.Token.IsCancellationRequested)
                                {
                                    try
                                    {
                                        
                                        IAudioDataProvider audioProvider = new AndroidAudioDataProvider();
                                        var transcriber = new ApiClientAudio(audioProvider, new WebSocketService());
                                        var transcription = await transcriber.StreamAudioDataAsync(matchedUser, _globalCts.Token, IsFirstRequest, PreviousResponseId);
                                        response = transcription.Response;

                                        if (response.Request == "none" && IsFirstRequest)
                                        {
                                            var voiceNone = await _voiceApiClient.GetVoiceByIdAsync(matchedUser.voiceId);
                                            var audioBytesNone = await textToSpeech.ConvertTextToSpeechAsync(voiceNone.voiceId, $"Вас не було розпізнано як користувача {matchedUser.userName}");
                                            await audioPlayerHelper.PlayAudioFromBytesAsync(audioBytesNone, _globalCts.Token);
                                        }
                                        
                                        if ((response.Request == "none" || !response.IsContinuous) && _globalCts.Token.IsCancellationRequested)
                                        {
                                            _prevResponseId = null;

                                            if (isPrivateConversation)
                                            {
                                                StopRecording();
                                                await restoreChatMessagesAction();
                                            }
                                            else
                                            {
                                                RestartListening(culture);
                                            }

                                            IsPrivateConversation = false;
                                            _hasCleared = false;
                                            return;
                                        }
                                        
                                        await Task.WhenAll(conversationIdTask);
                                        var createUserCmd = new CreateMessageCommand
                                        {
                                            ConversationId = conversationId ?? conversationIdTask.Result,
                                            Text = response.Request,
                                            UserRole = "user",
                                            Audio = transcription.Audio,
                                            LastRequestId = _prevResponseId,
                                            SubUserId = matchedUser.id
                                        };
                                        var createdUser = await _messagesApiClient.CreateMessageAsync(createUserCmd);
                                        chatMessageProgress.Report(new ChatMessage
                                        {
                                            Text = createdUser.text,
                                            UserRole = "user",
                                            LastRequestId = _prevResponseId,
                                            SubUserPhoto = matchedUser.photoPath,
                                            DateTimeCreated = createdUser.dateTimeCreated,
                                            URL = createdUser.audioPath
                                        });
                                        var UserName = $"(ім'я користувача - {matchedUser.userName})";
                                        ApiClientGptResponse answer = await _apiClientGPT.ContinueChatAsync($"{UserName}: {response.Request}", _prevResponseId);
                                        _prevResponseId = answer.responseId;

                                        var voice = await _voiceApiClient.GetVoiceByIdAsync(matchedUser.voiceId);

                                        var task = CalculatePrice(response.AudioDuration, answer, matchedUser.id, matchedUser.userId);
                                        IsContinueConversation = response.IsContinuous;
                                        IsFirstRequest = false;
                                        
                                        var audioBytes = await textToSpeech.ConvertTextToSpeechAsync(voice.voiceId, answer.text);

                                        var playAnswerTask = audioPlayerHelper.PlayAudioFromBytesAsync(audioBytes, _globalCts.Token);

                                        var careteMessageAI = new CreateMessageCommand()
                                        {
                                            ConversationId = conversationId ?? conversationIdTask.Result,
                                            Text = answer.text,
                                            UserRole = "ai",
                                            Audio = audioBytes,
                                            LastRequestId = _prevResponseId,
                                            SubUserId = matchedUser.id
                                        };
                                        var createdAItask = _messagesApiClient.CreateMessageAsync(careteMessageAI);
                                        await Task.WhenAll(createdAItask);
                                        chatMessageProgress.Report(new ChatMessage
                                        {
                                            Text = createdAItask.Result.text,
                                            UserRole = "ai",
                                            LastRequestId = _prevResponseId,
                                            SubUserPhoto = matchedUser.photoPath,
                                            DateTimeCreated = createdAItask.Result.dateTimeCreated,
                                            URL = createdAItask.Result.audioPath,
                                        });
                                
                                        await Task.WhenAll(playAnswerTask);
                                        await Task.Delay(100);
                                    }
                                    catch (Exception ex)
                                    {
                                        StopRecording();
                                        return;
                                    }
                                }
                                if (_globalCts.Token.IsCancellationRequested)
                                {
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
                    }
                };

                speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
                if (speechRecognizer is null)
                {
                    throw new ArgumentException("Speech recognizer is not available");
                }

                speechRecognizer.SetRecognitionListener(listener);
                StartListening(culture);
            {
                try
                {
                    return string.Empty;

                }
                catch (OperationCanceledException)
                {
                    StopRecording();
                    isListening = false;
                    speechRecognizer?.Destroy();
                    speechRecognizer = null;
                    await Toast.Make("Розпізнавання зупинено").Show(cancellationToken);
                    return string.Empty;
                }
            }
        }


        public async Task<string> ContinueListen(IProgress<string>? recognitionResult, IProgress<ChatMessage> chatMessageProgress, CancellationToken cancellationToken, Action clearChatMessagesAction, Func<Task> restoreChatMessagesAction, string prevResponseId, ContinueConversation continueConversation)
        {
            _isUserCancelled = false;

            cancellationToken.Register(() => CancelListening());

            _clearChatMessagesAction = clearChatMessagesAction;
            string lastTranscription = string.Empty;

            bool processingCommand = false;
            string conversationId = null;
            SubUserResponse? matchedUser = null;
            bool isPrivateConversation = false;

            matchedUser = continueConversation.SubUser;
            conversationId = continueConversation.ConversationId;

            if (matchedUser != null && !processingCommand)
            {
                processingCommand = true;

                try
                {
                    var audioPlayerHelper = new AudioPlayerHelper(new AudioManager());
                    await audioPlayerHelper.PlayAudioFromUrlAsync($"https://audioassistantblob.blob.core.windows.net/first-message/{matchedUser.id}.wav", _globalCts.Token);

                    IsContinueConversation = true;
                    IsFirstRequest = true;
                    TranscriptionResponse response = new();
                    var textToSpeech = new ElevenlabsApi();

                    Task<string> conversationIdTask;
                    if (isPrivateConversation)
                    {
                        conversationIdTask = _conversationApiClient.CreateConversationAsync("", matchedUser.id);
                        _prevResponseId = null;
                    }
                    else
                    {
                        conversationIdTask = _manageCacheData.GetConversationAsync();
                        _prevResponseId = prevResponseId;
                    }

                    while (IsContinueConversation&& !_globalCts.Token.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            IAudioDataProvider audioProvider = new AndroidAudioDataProvider();
                            var transcriber = new ApiClientAudio(audioProvider, new WebSocketService());
                            var transcription = await transcriber.StreamAudioDataAsync(matchedUser, _globalCts.Token, IsFirstRequest, PreviousResponseId);
                            response = transcription.Response;

                            if ((response.Request == "none" || !response.IsContinuous) && !isPrivateConversation)
                            {
                                StopRecording();
                                _prevResponseId = null;
                                await restoreChatMessagesAction();
                                IsPrivateConversation = false;
                                _hasCleared = false;
                                return lastTranscription;
                            }

                            await Task.WhenAll(conversationIdTask);
                            var createUserCmd = new CreateMessageCommand
                            {
                                ConversationId = conversationId ?? conversationIdTask.Result,
                                Text = response.Request,
                                UserRole = "user",
                                Audio = transcription.Audio,
                                LastRequestId = _prevResponseId,
                                SubUserId = matchedUser.id
                            };
                            var createdUser = await _messagesApiClient.CreateMessageAsync(createUserCmd);
                            chatMessageProgress.Report(new ChatMessage
                            {
                                Text = createdUser.text,
                                UserRole = "user",
                                LastRequestId = _prevResponseId,
                                SubUserPhoto = matchedUser.photoPath,
                                DateTimeCreated = createdUser.dateTimeCreated,
                                URL = createdUser.audioPath
                            });

                            ApiClientGptResponse answer = await _apiClientGPT.ContinueChatAsync($"(user name){matchedUser.userName}: {response.Request}", _prevResponseId);
                            _prevResponseId = answer.responseId;
                            var voiceTask =  _voiceApiClient.GetVoiceByIdAsync(matchedUser.voiceId);

                            var task = CalculatePrice(response.AudioDuration, answer, matchedUser.id, matchedUser.userId);
                            IsContinueConversation = response.IsContinuous;
                            IsFirstRequest = false;

                            await Task.WhenAll(voiceTask);

                            var audioBytes = await textToSpeech.ConvertTextToSpeechAsync(voiceTask.Result.voiceId, answer.text);
                            var playAnswerTask = audioPlayerHelper.PlayAudioFromBytesAsync(audioBytes, _globalCts.Token);

                            var careteMessageAI = new CreateMessageCommand()
                            {
                                ConversationId = conversationId ?? conversationIdTask.Result,
                                Text = answer.text,
                                UserRole = "ai",
                                Audio = audioBytes,
                                LastRequestId = _prevResponseId,
                                SubUserId = matchedUser.id
                            };
                            var createdAItask = _messagesApiClient.CreateMessageAsync(careteMessageAI);
                            await Task.WhenAll(createdAItask);
                            chatMessageProgress.Report(new ChatMessage
                            {
                                Text = createdAItask.Result.text,
                                UserRole = "ai",
                                LastRequestId = _prevResponseId,
                                SubUserPhoto = matchedUser.photoPath,
                                DateTimeCreated = createdAItask.Result.dateTimeCreated,
                                URL = createdAItask.Result.audioPath,
                            });

                            await Task.WhenAll(playAnswerTask);
                            await Task.Delay(100);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            StopRecording();
                            return lastTranscription;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Toast.Make("Помилка з listener: " + ex.Message).Show(cancellationToken);
                    return lastTranscription; ;
                }
            }
            StopRecording();
            return lastTranscription;
        }

        public void CancelListening()
        {
            _isUserCancelled = true;
            StopRecording();
            speechRecognizer?.Destroy();
            speechRecognizer = null;
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

            var appSettings = await _manageCacheData.GetAppSettingsAsync();
            var payment = await _manageCacheData.GetPaymentAsync();
            var userId = await SecureStorage.GetAsync("user_id");

            decimal totalCostDecimal = (decimal)(-totalCost);
            await _appSettingsApiClient.UpdateBalanceAsync(userId, totalCostDecimal, payment.maskedCardNumber, "");
            await _manageCacheData.UpdateAppSetttingsList();
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
            CancelListening();
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

    public static class DataProvider
    {
        public static IServiceProvider? Services { get; set; }

        public static IMediator Mediator =>
            Services!.GetRequiredService<IMediator>();

        public static ConversationApiClient ConversationApiClient =>
            Services!.GetRequiredService<ConversationApiClient>();

        public static ApiClientGPT ApiClientGPT =>
            Services!.GetRequiredService<ApiClientGPT>();

        public static MessagesApiClient MessagesApiClient =>
            Services!.GetRequiredService<MessagesApiClient>();

        public static ManageCacheData ManageCacheData =>
            Services!.GetRequiredService<ManageCacheData>();
        public static MoneyUsedApiClient MoneyUsedApiClient =>
            Services!.GetRequiredService<MoneyUsedApiClient>();
        public static MoneyUsersUsedApiClient MoneyUsersUsedApiClient =>
            Services!.GetRequiredService<MoneyUsersUsedApiClient>();

        public static VoiceApiClient VoiceApiClient =>
            Services!.GetRequiredService<VoiceApiClient>();

        public static PaymentHistoryApiClient PaymentHistoryApiClient =>
            Services!.GetRequiredService<PaymentHistoryApiClient>();

        public static AppSettingsApiClient AppSettingsApiClient =>
            Services!.GetRequiredService<AppSettingsApiClient>();
    }
}
