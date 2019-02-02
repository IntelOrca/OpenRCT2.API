using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using RethinkDb.Driver;
using System;
using System.Threading.Tasks;

namespace OpenRCT2.DB.Repositories
{
    internal class RctObjectRepsitory : IRctObjectRepository
    {
        private static readonly RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;

        public RctObjectRepsitory(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task<LegacyRctObject> GetLegacyFromNameAsync(string name)
        {
            var conn = await _dbService.GetConnectionAsync();
            return await R
                .Table(TableNames.LegacyObjects)
                .GetAllByIndex(nameof(LegacyRctObject.Name), name)
                .RunFirstOrDefaultAsync<LegacyRctObject>(conn);
        }

        public async Task UpdateLegacyAsync(LegacyRctObject legacyRctObject)
        {
            var conn = await _dbService.GetConnectionAsync();
            if (legacyRctObject.Id == null)
            {
                legacyRctObject.Id = Guid.NewGuid().ToString();
                await R
                    .Table(TableNames.LegacyObjects)
                    .Insert(legacyRctObject)
                    .RunResultAsync(conn);
            }
            else
            {
                var query = R
                    .Table(TableNames.LegacyObjects)
                    .Get(legacyRctObject.Id)
                    .Update(legacyRctObject);
                await query.RunResultAsync(conn);
            }
        }
    }
}
