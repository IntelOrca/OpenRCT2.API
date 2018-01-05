using System.Threading.Tasks;
using Amazon.DynamoDBv2;

namespace OpenRCT2.DB.Abstractions
{
    public interface IDBService
    {
        Task<IAmazonDynamoDB> GetClientAsync();
    }
}
