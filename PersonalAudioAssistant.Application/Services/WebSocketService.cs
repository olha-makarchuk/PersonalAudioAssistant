using System.Net.WebSockets;

namespace PersonalAudioAssistant.Application.Services
{
    public class WebSocketService
    {
        private readonly string wsUrl;
        private ClientWebSocket ws;

        public WebSocketService(string wsUrl)
        {
            this.wsUrl = wsUrl;
            ws = new ClientWebSocket();
        }

        public bool IsConnected => ws.State == WebSocketState.Open;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(wsUrl), cancellationToken);
        }

        public async Task SendDataAsync(byte[] buffer, int length, CancellationToken cancellationToken)
        {
            await ws.SendAsync(new ArraySegment<byte>(buffer, 0, length),
                WebSocketMessageType.Binary, true, cancellationToken);
        }

        public async Task<string> ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                        break;
                    }
                    if (result.Count > 0)
                    {
                        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine("Отримано повідомлення: " + message);
                        return message;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка при отриманні повідомлення: " + ex.Message);
                    return "Помилка при отриманні повідомлення";
                }
            }
            return "Помилка при отриманні повідомлення";
        }

        public async Task CloseConnectionAsync()
        {
            if (ws.State == WebSocketState.Open)
            {
                try
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Завершення з'єднання", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception під час CloseAsync: " + ex.Message);
                }
            }
        }
    }
}
