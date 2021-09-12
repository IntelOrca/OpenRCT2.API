using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenRCT2.DB.Models;

namespace OpenRCT2.DB.Abstractions
{
    public interface IContentRepository
    {
        Task<ContentItem> GetAsync(string id);
        Task<ContentItem> GetAsync(string ownerId, string name);
        Task<ContentItem[]> GetAllAsync(string owner);
        Task InsertAsync(ContentItem newsItem);
        Task UpdateAsync(ContentItem newsItem);
        Task DeleteAsync(string id);

        Task<bool> ExistsAsync(string ownerId, string name);
    }
}
