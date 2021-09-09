using System;
using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.DB.Repositories
{
    internal class ContentRepository : IContentRepository
    {
        public Task<ContentItem> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ContentItem newsItem)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string ownerId, string name)
        {
            throw new NotImplementedException();
        }
    }
}
