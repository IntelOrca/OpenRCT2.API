using System;
using System.Threading.Tasks;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Abstractions
{
    public interface IServerRepository
    {
        Task<Server[]> GetAll();
        Task<Server> GetByToken(string token);

        Task AddOrUpdate(Server server);
        Task Remove(Server server);
        Task RemoveDeadServers(DateTime minimumHeartbeatTime);
    }
}
