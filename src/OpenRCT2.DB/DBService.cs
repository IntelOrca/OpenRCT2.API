using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Options;
using OpenRCT2.DB.Abstractions;

namespace OpenRCT2.DB
{
    internal class DBService : IDBService
    {
        private readonly DBOptions _options;
        private readonly AmazonDynamoDBClient _client;

        public DBService(IOptions<DBOptions> options)
        {
            _options = options.Value;

            var region = RegionEndpoint.GetBySystemName(_options.AwsRegion);
            _client = new AmazonDynamoDBClient(_options.AwsAccessKeyId, _options.AwsSecretAccessKey, region);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public Task<IAmazonDynamoDB> GetClientAsync()
        {
            return Task.FromResult<IAmazonDynamoDB>(_client);
        }
    }
}
