using System;
using System.Threading.Tasks;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Abstractions
{
    public interface IServerRepository
    {
        Task<Server[]> GetAllAsync();
        Task<Server> GetByTokenAsync(string token);

        Task AddOrUpdateAsync(Server server);
        Task RemoveAsync(Server server);
        Task RemoveDeadServersAsync(DateTime minimumHeartbeatTime);
    }
}
