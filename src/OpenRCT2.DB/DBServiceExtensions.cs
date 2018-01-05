using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using OpenRCT2.DB.Abstractions;

namespace OpenRCT2.DB
{
    public static class DBServiceExtensions
    {
        public static Task<IAmazonDynamoDB> GetConnectionAsync(this IDBService dbService)
        {
            var dbServiceImpl = dbService as DBService;
            return dbServiceImpl.GetConnectionAsync();
        }
    }
}
