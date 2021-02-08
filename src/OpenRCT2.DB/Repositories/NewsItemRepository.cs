using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using RethinkDb.Driver;

namespace OpenRCT2.DB.Repositories
{
    internal class NewsItemRepository : INewsItemRepository
    {
        private static readonly RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;

        public NewsItemRepository(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task<NewsItemExtended[]> GetLatestAsync(int skip, int take, bool includeUnpublished)
        {
            var conn = await _dbService.GetConnectionAsync();

            var result = new List<NewsItemExtended>();
            if (includeUnpublished) {
                var subResult = await R
                    .Table(TableNames.NewsItems)
                    .Filter(r => r[nameof(NewsItem.Published)] == null)
                    .OrderBy(R.Desc(nameof(NewsItem.Created)))
                    // Join with users to get user name
                    .EqJoin(nameof(NewsItem.AuthorId), R.Table(TableNames.Users))
                    .Map(r => r["left"].Merge(new { AuthorName = r["right"][nameof(User.Name)] }))
                    .RunAtomAsync<NewsItemExtended[]>(conn);
                result.AddRange(subResult);
            }
            {
                var subResult = await R
                    .Table(TableNames.NewsItems)
                    .Filter(r => r[nameof(NewsItem.Published)] != null)
                    .OrderBy(R.Desc(nameof(NewsItem.Published)))
                    .Skip(skip)
                    .Limit(take)
                    // Join with users to get user name
                    .EqJoin(nameof(NewsItem.AuthorId), R.Table(TableNames.Users))
                    .Map(r => r["left"].Merge(new { AuthorName = r["right"][nameof(User.Name)] }))
                    .RunAtomAsync<NewsItemExtended[]>(conn);
                result.AddRange(subResult);
            }
            return result.ToArray();
        }

        public async Task<NewsItem> GetAsync(string id)
        {
            var conn = await _dbService.GetConnectionAsync();
            return await R
                .Table(TableNames.NewsItems)
                .Get(id)
                .RunAtomAsync<NewsItem>(conn);
        }

        public async Task UpdateAsync(NewsItem newsItem)
        {
            var conn = await _dbService.GetConnectionAsync();
            if (newsItem.Id == null)
            {
                newsItem.Id = Guid.NewGuid().ToString();
                await R
                    .Table(TableNames.NewsItems)
                    .Insert(newsItem)
                    .RunWriteAsync(conn);
            }
            else
            {
                var query = R
                    .Table(TableNames.NewsItems)
                    .Get(newsItem.Id)
                    .Update(newsItem);
                await query.RunWriteAsync(conn);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.NewsItems)
                .Get(id)
                .Delete();
            await query.RunWriteAsync(conn);
        }
    }
}
