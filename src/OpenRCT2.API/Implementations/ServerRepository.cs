using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Implementations
{
    public class ServerRepository : IServerRepository
    {
        private readonly ConcurrentDictionary<string, Server> _servers =
            new ConcurrentDictionary<string, Server>();

        public Task<Server[]> GetAllAsync()
        {
            return Task.FromResult(_servers.Values.ToArray());
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
                return Task.FromResult<Server>(null);
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
    }
}
