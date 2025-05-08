using Azure.Core;
using Newtonsoft.Json;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Contracts.SubUser;
using System.Text;
using System.Text.Json;

namespace PersonalAudioAssistant.Application.Services
{
    public class ApiClientAudio
    {
        private readonly WebSocketService webSocketService;
        private readonly IAudioDataProvider audioDataProvider;
        public ApiClientAudio(IAudioDataProvider audioDataProvider, WebSocketService webSocketService)
        {
            this.webSocketService = webSocketService;
            this.audioDataProvider = audioDataProvider;
        }

        public async Task<(TranscriptionResponse Response, byte[] Audio)> StreamAudioDataAsync(SubUserResponse subUser, CancellationToken cancellationToken, bool IsFirstRequest, string PreviousResponseId)
        {
            using var audioBuffer = new MemoryStream();
            try
            {
                await webSocketService.ConnectAsync(cancellationToken);

                var dataPayload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    subUser.endTime,
                    subUser.userVoice,
                    subUser.endPhrase,
                    IsFirstRequest,
                    PreviousResponseId
                });

                var idBytes = Encoding.UTF8.GetBytes(dataPayload);
                await webSocketService.SendDataAsync(idBytes, idBytes.Length, cancellationToken);

                string response = await webSocketService.ReceiveMessagesAsync(cancellationToken);

                if (response != "OK")
                {
                    await webSocketService.CloseConnectionAsync();
                    throw new Exception($"Помилка: сервер відхилив запит. Отримано відповідь: {response}");
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var stopSignal = new TaskCompletionSource<string>();

                //  Task для прийому повідомлень і перевірки STOP
                var receiveTask = Task.Run(async () =>
                {
                    while (!linkedCts.Token.IsCancellationRequested && webSocketService.IsConnected)
                    {
                        string message = await webSocketService.ReceiveMessagesAsync(linkedCts.Token);
                        if (message == "STOP")
                        {
                            stopSignal.TrySetResult(message);
                            break;
                        }
                    }
                }, linkedCts.Token);

                //  Task для відправлення аудіо
                var sendTask = Task.Run(async () =>
                {
                    while (!linkedCts.Token.IsCancellationRequested && webSocketService.IsConnected)
                    {
                        byte[] buffer = await audioDataProvider.GetAudioDataAsync(linkedCts.Token);
                        if (buffer?.Length > 0)
                        {
                            audioBuffer.Write(buffer, 0, buffer.Length);
                            await webSocketService.SendDataAsync(buffer, buffer.Length, linkedCts.Token);
                        }
                        await Task.Delay(50, linkedCts.Token);
                    }
                }, linkedCts.Token);

                // Чекаємо на STOP сигнал
                await stopSignal.Task;
                linkedCts.Cancel();

                // Отримуємо фінальний JSON-відповідь після STOP
                string finalResponseJson = await webSocketService.ReceiveMessagesAsync(cancellationToken);
                await webSocketService.CloseConnectionAsync();
                var finalResponse = JsonConvert.DeserializeObject<TranscriptionResponse>(finalResponseJson);
                byte[] originalAudio = audioBuffer.ToArray();

                int sampleRate = 44100;
                short bitsPerSample = 16;
                short channels = 1;
                int bytesPerSample = bitsPerSample / 8;             // 2
                int blockAlign = bytesPerSample * channels;     // 2
                int bytesPerSecond = sampleRate * blockAlign;       // 88200

                // 2. Розрахунок вирівняних обрізок з початку
                double rawStartSec = Math.Max(0.0, finalResponse.First_detected_time - 1.0);
                int trimStartBytes = (int)(rawStartSec * bytesPerSecond);
                trimStartBytes = (trimStartBytes / blockAlign) * blockAlign;

                int trimEndBytes = 0;
                if (subUser.endTime != null)
                {
                    int trimEndDurationSec = Convert.ToInt32(subUser.endTime);
                    trimEndBytes = trimEndDurationSec * bytesPerSecond;
                    trimEndBytes = (trimEndBytes / blockAlign) * blockAlign;
                }
                else
                {
                    int trimEndDurationSec = 2;
                    trimEndBytes = trimEndDurationSec * bytesPerSecond;
                    trimEndBytes = (trimEndBytes / blockAlign) * blockAlign;
                }
                // 3. Обрізка
                int availableBytes = originalAudio.Length - trimStartBytes - trimEndBytes;
                if (availableBytes <= 0)
                    return (finalResponse, Array.Empty<byte>());

                byte[] trimmedAudio = new byte[availableBytes];
                Array.Copy(originalAudio, trimStartBytes, trimmedAudio, 0, availableBytes);

                return (finalResponse, trimmedAudio);
            }
            catch (Exception ex)
            {
                await webSocketService.CloseConnectionAsync();
                return (new TranscriptionResponse
                {
                }, Array.Empty<byte>());
            }
            finally
            {
                if (audioDataProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    public class TranscriptionResponse
    {
        public string Request { get; set; }

        public double AudioDuration { get; set; }
        public bool IsContinuous { get; set; }
        public double First_detected_time { get; set; }
    }
}