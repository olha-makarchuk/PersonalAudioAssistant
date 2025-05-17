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

        public async Task<(TranscriptionResponse Response, byte[] Audio)> StreamAudioDataAsync(
            SubUserResponse subUser,
            CancellationToken cancellationToken,
            bool IsFirstRequest,
            string PreviousResponseId)
        {
            using var audioBuffer = new MemoryStream();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var stopSignal = new TaskCompletionSource<string>();

            try
            {
                await webSocketService.ConnectAsync(linkedCts.Token);

                var dataPayload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    subUser.endTime,
                    subUser.userVoice,
                    subUser.endPhrase,
                    IsFirstRequest,
                    PreviousResponseId
                });

                var idBytes = Encoding.UTF8.GetBytes(dataPayload);
                await webSocketService.SendDataAsync(idBytes, idBytes.Length, linkedCts.Token);

                string response = await webSocketService.ReceiveMessagesAsync(linkedCts.Token);
                if (response != "OK")
                {
                    await webSocketService.CloseConnectionAsync();
                    throw new Exception($"Помилка: сервер відхилив запит. Отримано відповідь: {response}");
                }

                // 🟢 Task для прийому STOP
                var receiveTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!linkedCts.Token.IsCancellationRequested && webSocketService.IsConnected)
                        {
                            linkedCts.Token.ThrowIfCancellationRequested();

                            var receive = webSocketService.ReceiveMessagesAsync(linkedCts.Token);
                            var completed = await Task.WhenAny(receive, Task.Delay(10000, linkedCts.Token));

                            if (completed == receive)
                            {
                                string message = await receive;
                                if (message == "STOP")
                                {
                                    stopSignal.TrySetResult(message);
                                    break;
                                }
                            }
                            else
                            {
                                // таймаут очікування
                                throw new TimeoutException("ReceiveMessagesAsync timed out.");
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        stopSignal.TrySetException(ex);
                    }
                }, linkedCts.Token);

                // 🟢 Task для надсилання аудіо
                var sendTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!linkedCts.Token.IsCancellationRequested && webSocketService.IsConnected)
                        {
                            linkedCts.Token.ThrowIfCancellationRequested();

                            byte[] buffer = await audioDataProvider.GetAudioDataAsync(linkedCts.Token);
                            if (buffer?.Length > 0)
                            {
                                audioBuffer.Write(buffer, 0, buffer.Length);
                                await webSocketService.SendDataAsync(buffer, buffer.Length, linkedCts.Token);
                            }

                            await Task.Delay(50, linkedCts.Token);
                        }
                    }
                    catch (OperationCanceledException) { }
                }, linkedCts.Token);

                // 🟢 Очікуємо STOP або скасування
                await stopSignal.Task;
                linkedCts.Cancel(); // Зупиняємо все

                // 🟢 Отримуємо фінальну відповідь
                string finalResponseJson = await webSocketService.ReceiveMessagesAsync(cancellationToken);
                await webSocketService.CloseConnectionAsync();

                var finalResponse = JsonConvert.DeserializeObject<TranscriptionResponse>(finalResponseJson);
                byte[] originalAudio = audioBuffer.ToArray();

                // 🔧 Обрізання аудіо
                int sampleRate = 44100;
                short bitsPerSample = 16;
                short channels = 1;
                int bytesPerSample = bitsPerSample / 8;
                int blockAlign = bytesPerSample * channels;
                int bytesPerSecond = sampleRate * blockAlign;

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
                    trimEndBytes = 2 * bytesPerSecond;
                    trimEndBytes = (trimEndBytes / blockAlign) * blockAlign;
                }

                int availableBytes = originalAudio.Length - trimStartBytes - trimEndBytes;
                if (availableBytes <= 0)
                    return (finalResponse, Array.Empty<byte>());

                byte[] trimmedAudio = new byte[availableBytes];
                Array.Copy(originalAudio, trimStartBytes, trimmedAudio, 0, availableBytes);

                return (finalResponse, trimmedAudio);
            }
            catch (OperationCanceledException)
            {
                await webSocketService.CloseConnectionAsync();
                return (new TranscriptionResponse { }, Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                await webSocketService.CloseConnectionAsync();
                return (new TranscriptionResponse { }, Array.Empty<byte>());
            }
            finally
            {
                if (audioDataProvider is IDisposable disposable)
                    disposable.Dispose();
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