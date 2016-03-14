using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenRCT2.API.Extensions;
using OpenRCT2.Network;

namespace OpenRCT2.API
{
    public class WebSocketSession : IDisposable
    {
        private const string ErrorColour = "red";

        private ILogger<WebSocketSession> _logger;
        private WebSocket _webSocket;
        private OpenRCT2Client _gameClient;
        private bool _shouldClose;

        public WebSocketSession(IServiceProvider serviceProvider, WebSocket webSocket)
        {
            
            _logger = serviceProvider.GetService<ILogger<WebSocketSession>>();
            _webSocket = webSocket;

            _logger.LogInformation("WebSocket connection opened");
        }

        public void Dispose()
        {
            if (_gameClient != null)
            {
                _gameClient.Dispose();
            }
        }

        public async Task Run()
        {
            byte[] buffer = new byte[1024 * 4];
            var segment = new ArraySegment<byte>(buffer);

            Task<WebSocketReceiveResult> receiveTask = null;
            bool closed = false;
            do
            {
                if (_shouldClose)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected", CancellationToken.None);
                    closed = true;
                }
                else
                {
                    if (_gameClient != null)
                    {
                        // Check if game connection is still live
                        if (_gameClient.ConnectionFailed)
                        {
                            string connExceptionMessage = _gameClient.ConnectionException.Message;
                            string message = $"Unable to connect to server: {connExceptionMessage}";
                            _logger.LogInformation(message);
                            await Send(GetMessageAsHtml(message, ErrorColour));
                            _shouldClose = true;
                        }
                    }

                    if (receiveTask == null)
                    {
                        receiveTask = _webSocket.ReceiveAsync(segment, CancellationToken.None);
                    }
                    else if (receiveTask.IsCompleted)
                    {
                        WebSocketReceiveResult result = receiveTask.Result;
                        closed = result.CloseStatus.HasValue;
                        if (closed)
                        {
                            await _webSocket.CloseAsync(result.CloseStatus.Value,
                                                        result.CloseStatusDescription,
                                                        CancellationToken.None);
                            closed = true;
                        }
                        else
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            await OnReceiveMessage(message);
                        }
                        receiveTask = null;
                    }
                }
            } while (!closed);

            _logger.LogInformation("WebSocket connection closed");
        }

        private async Task<bool> Connect(string host, int port, string userName, string password = null)
        {
            _logger.LogInformation("Connecting to {0}:{1} as {2}", host, port, userName);

            try
            {
                _gameClient = new OpenRCT2Client();
                _gameClient.ChatMessageReceived += async (object sender, IOpenRCT2String e) => {
                    await Send(e.ToHtml());
                };

                await _gameClient.Connect(host, port);

                AuthenticationResult result = await _gameClient.Authenticate(userName, password);
                if (result != AuthenticationResult.OK)
                {
                    await SendError("Access denied: " + result);
                    return false;
                }

                return true;
            }
            catch (SocketException ex)
            {
                await SendError("Unable to connect to server: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await SendError("An error occured whilst connecting to the server.");
            }
            return false;
        }

        private async Task Send(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore
            });
            await _webSocket.SendAsync(json);
        }

        private async Task Send(string message)
        {
            await Send(new JsonMessage() {
                type = "chat",
                text = message
            });
        }

        private async Task SendError(string message)
        {
            await Send(GetMessageAsHtml(message, ErrorColour));
        }

        private async Task OnReceiveMessage(string message)
        {
            var jsonMessage = JsonConvert.DeserializeObject<JsonMessage>(message);
            switch (jsonMessage.type) {
            case "connect":
                string server;
                int port;

                if (TryParseHost(jsonMessage.host, out server, out port))
                {
                    if (!await Connect(server, port, jsonMessage.userName, jsonMessage.password))
                    {
                        _shouldClose = true;
                    }
                }
                else
                {
                    await SendError("Unable to connect to server.");
                    _shouldClose = true;
                }
                break;
            case "chat":
                IOpenRCT2String openRCT2string = new OpenRCT2String(jsonMessage.text);
                _gameClient.SendChat(openRCT2string);
                break;
            }
        }

        private static bool TryParseHost(string host, out string server, out int port)
        {
            int seperatorIndex = host.IndexOf(":");
            if (seperatorIndex == -1)
            {
                server = host;
                port = OpenRCT2Client.DefaultPort;
            }
            else
            {
                server = host.Substring(0, seperatorIndex);
                string szPort = host.Substring(seperatorIndex + 1);
                if (!Int32.TryParse(szPort, out port))
                {
                    return false;
                }
            }
            return true;
        }

        private static string GetMessageAsHtml(string message, string colour)
        {
            return $"<span style=\"color: {colour};\">{message}.</span>";
        }

        public class JsonMessage
        {
            public string type { get; set; }
            public string host { get; set; }
            public string password { get; set; }
            public string userName { get; set; }

            public string text { get; set; }
        }
    }
}
