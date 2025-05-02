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

        public async Task<(string Response, byte[] Audio)> StreamAudioDataAsync(SubUserResponse subUser, CancellationToken cancellationToken, bool IsFirstRequest, string PreviousResponseId)
        {
            using var audioBuffer = new MemoryStream();
            try
            {
                await webSocketService.ConnectAsync(cancellationToken);

                var dataPayload = JsonSerializer.Serialize(new
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

                return (finalResponseJson, audioBuffer.ToArray());
            }
            catch (Exception ex)
            {
                await webSocketService.CloseConnectionAsync();
                return ("Помилка при стрімінгу: " + ex.Message, audioBuffer.ToArray());
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
}