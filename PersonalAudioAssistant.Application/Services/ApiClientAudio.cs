using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Domain.Entities;
using System.Text;
using System.Text.Json;

namespace PersonalAudioAssistant.Application.Services
{
    public class ApiClientAudio
    {
        private readonly WebSocketService webSocketService;
        private readonly IAudioDataProvider audioDataProvider;

        public ApiClientAudio(string wsUrl, IAudioDataProvider audioDataProvider)
        {
            webSocketService = new WebSocketService(wsUrl);
            this.audioDataProvider = audioDataProvider;
        }

        public async Task<string> StreamAudioDataAsync(SubUser subUser, CancellationToken cancellationToken)
        {
            try
            {
                await webSocketService.ConnectAsync(cancellationToken);

                var dataPayload = JsonSerializer.Serialize(new { subUser.UserId, subUser.VoiceId });
                var idBytes = Encoding.UTF8.GetBytes(dataPayload);
                await webSocketService.SendDataAsync(idBytes, idBytes.Length, cancellationToken);

                string response = await webSocketService.ReceiveMessagesAsync(cancellationToken);

                if (response != "OK")
                {
                    await webSocketService.CloseConnectionAsync();
                    throw new Exception($"Помилка: сервер відхилив запит. Отримано відповідь: {response}");
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                var receiveTask = webSocketService.ReceiveMessagesAsync(linkedCts.Token);

                var sendTask = Task.Run(async () =>
                {
                    while (!linkedCts.Token.IsCancellationRequested && webSocketService.IsConnected)
                    {
                        byte[] buffer = await audioDataProvider.GetAudioDataAsync(linkedCts.Token);
                        if (buffer?.Length > 0)
                        {
                            await webSocketService.SendDataAsync(buffer, buffer.Length, linkedCts.Token);
                        }
                        await Task.Delay(50, linkedCts.Token);
                    }
                }, linkedCts.Token);

                await Task.WhenAny(sendTask, receiveTask);

                var finishedTask = await Task.WhenAny(sendTask, receiveTask);

                linkedCts.Cancel();

                string finalResponse = await receiveTask;
                await webSocketService.CloseConnectionAsync();

                return finalResponse;
            }
            catch (Exception ex)
            {
                await webSocketService.CloseConnectionAsync();
                return "Помилка при стрімінгу: " + ex.Message;
            }
            finally
            {
                // Зупиняємо запис аудіо, якщо audioDataProvider реалізує IDisposable
                if (audioDataProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
