using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Extensions;
using OpenRCT2.API.Implementations;
using OpenRCT2.API.JsonModels;
using OpenRCT2.API.Models;
using OpenRCT2.Network;

namespace OpenRCT2.API.Controllers
{
    public class ServerController : Controller
    {
        private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(75);

        #region Request / Response Models

        public class JGetServersResponse : JResponse
        {
            public JServer[] servers { get; set; }
        }

        public class JAdvertiseServerRequest
        {
            public string key { get; set; }
            public int port { get; set; }
        }

        public class JAdvertiseServerResponse : JResponse
        {
            public string token { get; set; }
        }

        #endregion

        public class JAdvertiseHeartbeatRequest
        {
            public string token { get; set; }
            public int players { get; set; }
            public JGameInfo gameInfo { get; set; }
        }

        [Route("servers")]
        [HttpGet]
        public async Task<IJResponse> GetServers(
            [FromServices] IServerRepository serverRepository)
        {
            Server[] servers = await serverRepository.GetAll();
            JServer[] jServers = servers.Select(x => JServer.FromServer(x))
                                        .ToArray();

            await DoServerCleanup(serverRepository);
            var response = new JGetServersResponse()
            {
                status = JStatus.OK,
                servers = jServers
            };
            return response;
        }

        [Route("servers")]
        [HttpPost]
        public async Task<IJResponse> AdvertiseServer(
            [FromServices] IServerRepository serverRepository,
            [FromServices] Random random,
            [FromBody] JAdvertiseServerRequest body)
        {
            var remoteAddress = "localhost";
            JServerInfo serverInfo;

            try
            {
                string serverInfoJson;
                using (var client = new OpenRCT2Client())
                {
                    await client.Connect(remoteAddress, body.port);
                    serverInfoJson = await client.RequestServerInfo();
                }
                serverInfo = JsonConvert.DeserializeObject<JServerInfo>(serverInfoJson);
            }
            catch (SocketException)
            {
                return JResponse.Error("Unable to connect to server, make sure your ports are open.");
            }
            catch (TimeoutException)
            {
                return JResponse.Error("Timed out while waiting for server response.");
            }
            catch
            {
                return JResponse.Error("Unable to advertise server.");
            }

            var token = random.NextBytes(8)
                              .ToHexString();
            var server = new Server()
            {
                Token = token,
                LastHeartbeat = DateTime.Now,

                Addresses = new ServerAddressList()
                {
                    IPv4 = new string[] { remoteAddress },
                    IPv6 = new string[0]
                },
                Port = body.port,
                Name = serverInfo.name,
                Description = serverInfo.description,
                Provider = serverInfo.provider,
                RequiresPassword = serverInfo.requiresPassword,
                Players = serverInfo.players,
                MaxPlayers = serverInfo.maxPlayers,
                Version = serverInfo.version
            };
            await serverRepository.AddOrUpdate(server);

            var response = new JAdvertiseServerResponse()
            {
                status = JStatus.OK,
                token = token
            };
            return response;
        }

        [Route("servers")]
        [HttpPut]
        public async Task<IJResponse> AdvertiseHeartbeat(
            [FromServices] IServerRepository serverRepository,
            [FromBody] JAdvertiseHeartbeatRequest body)
        {
            Server server = await serverRepository.GetByToken(body.token);
            if (server == null)
            {
                return JResponse.Error("Server not registered.");
            }

            server.Players = body.players;
            server.GameInfo = body.gameInfo;
            server.LastHeartbeat = DateTime.Now;
            await serverRepository.AddOrUpdate(server);

            return JResponse.OK();
        }

        private static Task DoServerCleanup(IServerRepository serverRepository)
        {
            DateTime minimumHeartbeatTime = DateTime.Now
                                                    .Subtract(HeartbeatTimeout);
            return serverRepository.RemoveDeadServers(minimumHeartbeatTime);
        }
    }
}
