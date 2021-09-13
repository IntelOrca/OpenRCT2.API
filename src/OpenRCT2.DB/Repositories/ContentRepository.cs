using System.Linq;
using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using RethinkDb.Driver;
using RethinkDb.Driver.Ast;

namespace OpenRCT2.DB.Repositories
{
    internal class ContentRepository : IContentRepository
    {
        private static readonly RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;

        public ContentRepository(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task<ContentItem> GetAsync(string id)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Content)
                .Get(id);
            var result = await query.RunAtomAsync<ContentItem>(conn);
            return result;
        }

        public async Task<ContentItem> GetAsync(string ownerId, string name)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Content)
                .GetAllByIndex(nameof(ContentItem.NormalisedName), name.ToLowerInvariant())
                .Filter(r => r[nameof(ContentItem.OwnerId)] == ownerId)
                .Nth(0)
                .Default_(null as object);
            var result = await query.RunAtomAsync<ContentItem>(conn);
            return result;
        }

        public async Task<ContentItemExtended[]> GetAllAsync(ContentQuery query)
        {
            var conn = await _dbService.GetConnectionAsync();
            var q = R
                .Table(TableNames.Content);
            var q2 = query.OwnerId != null ?
                (ReqlExpr)q.GetAllByIndex(nameof(ContentItem.OwnerId), query.OwnerId) :
                (ReqlExpr)q;
            var q3 = q2
                .EqJoin(nameof(ContentItem.OwnerId), R.Table(TableNames.Users))
                .Map(r => r["left"].Merge(new { Owner = r["right"][nameof(User.Name)] }));
            var q4 = q3.OrderBy(R.Desc(nameof(ContentItem.Created)));
            var results = await q4.RunAtomAsync<ContentItemExtended[]>(conn);
            return results.ToArray();
        }

        public async Task UpdateAsync(ContentItem item)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Content)
                .Get(item.Id)
                .Update(item);
            await query.RunWriteAsync(conn);
        }

        public async Task InsertAsync(ContentItem item)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Content)
                .Insert(item);
            await query.RunWriteAsync(conn);
        }

        public async Task DeleteAsync(string id)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Content)
                .Get(id)
                .Delete();
            await query.RunWriteAsync(conn);
        }

        public async Task<bool> ExistsAsync(string ownerId, string name)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Content)
                .GetAllByIndex(nameof(ContentItem.OwnerId), ownerId)
                .Contains(r => r[nameof(ContentItem.NormalisedName)] == name.ToLowerInvariant());
            var result = await query.RunAtomAsync<bool>(conn);
            return result;
        }
    }
}
