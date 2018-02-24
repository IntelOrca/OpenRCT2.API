using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.JsonModels;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Implementations
{
    public class ServerRepository : IServerRepository
    {
        private readonly ConcurrentDictionary<string, Server> _servers =
            new ConcurrentDictionary<string, Server>();

        public async Task<Server[]> GetAllAsync()
        {
            Server[] legacyServers = await GetLegacyServersAsync();
            Server[] allServers = _servers.Values.Concat(legacyServers)
                                                 .ToArray();
            return allServers;
        }

        public Task<Server> GetByTokenAsync(string token)
        {
            Server server;
            if (_servers.TryGetValue(token, out server))
            {
                return Task.FromResult(server);
            }
            else
            {
                return null;
            }
        }

        public Task AddOrUpdateAsync(Server server)
        {
            _servers[server.Token] = server;
            return Task.FromResult(0);
        }

        public Task RemoveAsync(Server server)
        {
            _servers.TryRemove(server.Token, out server);
            return Task.FromResult(0);
        }

        public async Task RemoveDeadServersAsync(DateTime minimumHeartbeatTime)
        {
            IEnumerable<Server> servers = await GetAllAsync();
            servers = servers.Where(x => x.Token != null)
                             .Where(x => x.LastHeartbeat < minimumHeartbeatTime);
            foreach (Server server in servers)
            {
                await RemoveAsync(server);
            }
        }

        private async Task<Server[]> GetLegacyServersAsync()
        {
            const string LegacyUrl = "https://servers.openrct2.website";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders
                          .Accept
                          .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetAsync(LegacyUrl);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        var jsonResponse = new { status = "", servers = new JServer[0] };
                        jsonResponse = JsonConvert.DeserializeAnonymousType(content, jsonResponse);
                        return jsonResponse.servers.Select(x => x.ToServer())
                                                   .ToArray();
                    }
                }
            }
            catch
            {
            }
            return Array.Empty<Server>();
        }
    }
}
