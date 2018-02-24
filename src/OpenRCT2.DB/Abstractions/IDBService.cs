using System.Threading.Tasks;
using RethinkDb.Driver.Net;

namespace OpenRCT2.DB.Abstractions
{
    public interface IDBService
    {
        Task SetupAsync();
        Task<IConnection> GetConnectionAsync();
    }
}
