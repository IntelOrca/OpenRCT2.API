using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.DB.Repositories
{
    internal class RctObjectRepository : IRctObjectRepository
    {
        private readonly IDBService _dbService;

        public RctObjectRepository(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task<RctObject[]> GetAllAsync()
        {
            var client = await _dbService.GetClientAsync();
            var req = new ScanRequest(TableNames.Objects);
            var result = await client.ScanAsync(req);
            return FromItems(result.Items);
        }

        public async Task<RctObject[]> ByNameAsync(string name)
        {
            var client = await _dbService.GetClientAsync();
            var req = new QueryRequest(TableNames.Objects);
            req.IndexName = "Name-index";
            req.KeyConditionExpression = $"#n_name = :v_name";
            req.ExpressionAttributeNames = new Dictionary<string, string>()
            {
                { "#n_name", "Name" }
            };
            req.ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                { ":v_name", new AttributeValue(name) }
            };
            var result = await client.QueryAsync(req);
            return FromItems(result.Items);
        }

        public async Task<RctObject[]> ByHeaderAsync(string header)
        {
            var client = await _dbService.GetClientAsync();
            var req = new QueryRequest(TableNames.Objects);
            req.IndexName = "Header-index";
            req.KeyConditionExpression = $"#n_header = :v_header";
            req.ExpressionAttributeNames = new Dictionary<string, string>()
            {
                { "#n_header", "Header" }
            };
            req.ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                { ":v_header", new AttributeValue(header) }
            };
            var result = await client.QueryAsync(req);
            return FromItems(result.Items);
        }

        private RctObject[] FromItems(List<Dictionary<string, AttributeValue>> items)
        {
            return items
                .Select(x => FromItem(x))
                .ToArray();
        }

        private RctObject FromItem(Dictionary<string, AttributeValue> item)
        {
            return new RctObject()
            {
                Id = GetString(item, "Id"),
                Name = GetString(item, "Name"),
                Header = GetString(item, "Header"),
                DownloadAddress = GetString(item, "DownloadAddress"),
            };
        }

        private static string GetString(Dictionary<string, AttributeValue> item, string key)
        {
            if (item.TryGetValue(key, out var value))
            {
                return value.S;
            }
            else
            {
                return null;
            }
        }
    }
}
