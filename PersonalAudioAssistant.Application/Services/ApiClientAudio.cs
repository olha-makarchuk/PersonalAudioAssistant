using PersonalAudioAssistant.Application.Interfaces;
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

        public async Task<string> StreamAudioDataAsync(string idUser, string voiceId, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Підключення до сервера для стрімінгу...");
                await webSocketService.ConnectAsync(cancellationToken);
                Console.WriteLine("Підключено. Розпочато стрімінг аудіо...");

                var idPayload = JsonSerializer.Serialize(new { idUser, voiceId });
                var idBytes = Encoding.UTF8.GetBytes(idPayload);
                await webSocketService.SendDataAsync(idBytes, idBytes.Length, cancellationToken);

                string response = await webSocketService.ReceiveMessagesAsync(cancellationToken);
                if (response != "OK")
                {
                    Console.WriteLine("Сервер відхилив запит. Перериваємо стрімінг.");
                    await webSocketService.CloseConnectionAsync();
                    return "Помилка: сервер відхилив запит";
                }

                Console.WriteLine("Ідентифікатор прийнято. Розпочато стрімінг аудіо...");

                var receiveTask = Task.Run(() => webSocketService.ReceiveMessagesAsync(cancellationToken), cancellationToken);
                var sendTask = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested && webSocketService.IsConnected)
                    {
                        byte[] buffer = await audioDataProvider.GetAudioDataAsync(cancellationToken);
                        if (buffer?.Length > 0)
                        {
                            await webSocketService.SendDataAsync(buffer, buffer.Length, cancellationToken);
                        }
                        await Task.Delay(50, cancellationToken);
                    }
                }, cancellationToken);

                var finishedTask = await Task.WhenAny(sendTask, receiveTask);

                if (finishedTask == receiveTask)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string finalResponse = await receiveTask;
                    await webSocketService.CloseConnectionAsync();
                    return finalResponse;
                }
                else
                {
                    string finalResponse = await receiveTask;
                    await webSocketService.CloseConnectionAsync();
                    return finalResponse;
                }
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
