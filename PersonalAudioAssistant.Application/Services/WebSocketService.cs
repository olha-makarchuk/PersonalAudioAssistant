using System.Net.WebSockets;
using System.Text;

namespace PersonalAudioAssistant.Application.Services
{
    public class WebSocketService : IDisposable
    {
        private ClientWebSocket ws;
        private bool disposed = false;
        //private string _wsUrl = "ws://10.0.2.2:8000/ws/audio";
        private string _wsUrl = "ws://192.168.0.155:8060/ws/audio";

        public WebSocketService()
        {
            ws = new ClientWebSocket();
        }

        public bool IsConnected => ws != null && ws.State == WebSocketState.Open;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            DisposeWebSocket();
            ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(_wsUrl), cancellationToken);
        }

        public async Task SendDataAsync(byte[] buffer, int length, CancellationToken cancellationToken)
        {
            if (!IsConnected)
                throw new InvalidOperationException("WebSocket не підключено.");

            await ws.SendAsync(new ArraySegment<byte>(buffer, 0, length),
                WebSocketMessageType.Binary, endOfMessage: true, cancellationToken: cancellationToken);
        }

        public async Task<string> ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            if (!IsConnected)
                throw new InvalidOperationException("WebSocket не підключено.");

            var buffer = new byte[1024];
            try
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                    throw new WebSocketException("WebSocket закритий віддаленим хостом.");
                }
                if (result.Count > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    //Console.WriteLine("Отримано повідомлення: " + message);
                    return message;
                }
                else
                {
                    throw new Exception("Отримано 0 байт даних.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Помилка при отриманні повідомлення", ex);
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (ws != null && ws.State == WebSocketState.Open)
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
            DisposeWebSocket();
        }

        private void DisposeWebSocket()
        {
            if (ws != null)
            {
                ws.Dispose();
                ws = null;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                DisposeWebSocket();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
