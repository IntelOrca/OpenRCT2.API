using System;
using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using RethinkDb.Driver;

namespace OpenRCT2.DB.Repositories
{
    internal class RctObjectRepository : IRctObjectRepository
    {
        private static readonly RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;

        public RctObjectRepository(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task<LegacyRctObject> GetLegacyFromNameAsync(string name)
        {
            var conn = await _dbService.GetConnectionAsync();
            return await R
                .Table(TableNames.LegacyObjects)
                .GetAllByIndex(nameof(LegacyRctObject.Name), name)
                .Min(nameof(LegacyRctObject.NeDesignId))
                .Default_(null as object)
                .RunAtomAsync<LegacyRctObject>(conn);
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
                    .RunWriteAsync(conn);
            }
            else
            {
                var query = R
                    .Table(TableNames.LegacyObjects)
                    .Get(legacyRctObject.Id)
                    .Update(legacyRctObject);
                await query.RunWriteAsync(conn);
            }
        }

        public async Task<LegacyRctObject> GetLegacyObjectWithHighestNeIdAsync()
        {
            var conn = await _dbService.GetConnectionAsync();
            var hmm = await R
                .Table(TableNames.LegacyObjects)
                .Max(nameof(LegacyRctObject.NeDesignId))
                .Default_(null as object)
                .RunAtomAsync<LegacyRctObject>(conn);
            return hmm;
        }
    }
}
