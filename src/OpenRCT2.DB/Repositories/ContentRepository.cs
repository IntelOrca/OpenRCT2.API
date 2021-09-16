using System;
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
            q2 = q2
                .EqJoin(nameof(ContentItem.OwnerId), R.Table(TableNames.Users))
                .Map(r => r["left"].Merge(new { Owner = r["right"][nameof(User.Name)] }));
            if (query.CurrentUserId != null)
            {
                q2 = q2.Merge(c => new
                {
                    HasLiked = R
                        .Table(TableNames.ContentLikes)
                        .GetAllByIndex(nameof(ContentLike.ContentId), c["id"])
                        .Map(l => l[nameof(ContentLike.UserId)])
                        .Contains(query.CurrentUserId)
                });
            }
            if (query.SortBy == ContentSortKind.DateCreated)
            {
                q2 = q2.OrderBy(R.Desc(nameof(ContentItem.Created)));
            }
            else
            {
                q2 = q2.OrderBy(R.Desc(nameof(ContentItem.LikeCount)));
            }
            var results = await q2.RunAtomAsync<ContentItemExtended[]>(conn);
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

        public async Task<bool> GetUserLikeAsync(string contentId, string userId)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.ContentLikes)
                .GetAllByIndex(nameof(ContentLike.ContentId), contentId)
                .Contains(r => r[nameof(ContentLike.UserId)] == userId);
            var result = await query.RunAtomAsync<bool>(conn);
            return result;
        }

        public async Task SetUserLikeAsync(string contentId, string userId, bool value)
        {
            var conn = await _dbService.GetConnectionAsync();
            var currentValue = await GetUserLikeAsync(contentId, userId);
            if (value && !currentValue)
            {
                var like = new ContentLike()
                {
                    Id = Guid.NewGuid().ToString(),
                    ContentId = contentId,
                    UserId = userId,
                    When = DateTime.UtcNow
                };
                var query = R
                    .Table(TableNames.ContentLikes)
                    .Insert(like);
                await query.RunWriteAsync(conn);

                var updateCountQuery = R
                    .Table(TableNames.Content)
                    .Get(contentId)
                    .Update(r => new { LikeCount = r[nameof(ContentItem.LikeCount)].Add(1) });
                await updateCountQuery.RunWriteAsync(conn);
            }
            else if (!value && currentValue)
            {
                var query = R
                    .Table(TableNames.ContentLikes)
                    .GetAllByIndex(nameof(ContentLike.ContentId), contentId)
                    .Filter(r => r[nameof(ContentLike.UserId)] == userId)
                    .Delete();
                await query.RunWriteAsync(conn);

                var updateCountQuery = R
                    .Table(TableNames.Content)
                    .Get(contentId)
                    .Update(r => new { LikeCount = r[nameof(ContentItem.LikeCount)].Sub(1) });
                await updateCountQuery.RunWriteAsync(conn);
            }
        }

        public async Task IncrementDownloadCountAsync(string contentId)
        {
            var conn = await _dbService.GetConnectionAsync();
            var updateCountQuery = R
                .Table(TableNames.Content)
                .Get(contentId)
                .Update(r => new { DownloadCount = r[nameof(ContentItem.DownloadCount)].Add(1).Default_(1) });
            await updateCountQuery.RunWriteAsync(conn);
        }
    }
}
