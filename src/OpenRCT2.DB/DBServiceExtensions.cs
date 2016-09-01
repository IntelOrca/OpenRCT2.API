using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using RethinkDb.Driver.Net;

namespace OpenRCT2.DB
{
    public static class DBServiceExtensions
    {
        public static Task<IConnection> GetConnectionAsync(this IDBService dbService)
        {
            var dbServiceImpl = dbService as DBService;
            return dbServiceImpl.GetConnectionAsync();
        }
    }
}
