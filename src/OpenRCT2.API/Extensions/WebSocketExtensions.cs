using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRCT2.API.Extensions
{
    public static class WebSocketExtensions
    {
        public static Task SendAsync(this WebSocket webSocket, string text, CancellationToken ct = default(CancellationToken))
        {
            byte[] payload = Encoding.UTF8.GetBytes(text);
            var buffer = new ArraySegment<byte>(payload);
            return webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, ct);
        }
    }
}
