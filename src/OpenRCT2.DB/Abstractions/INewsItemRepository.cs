using System.Threading.Tasks;
using OpenRCT2.DB.Models;

namespace OpenRCT2.DB.Abstractions
{
    public interface INewsItemRepository
    {
        Task<NewsItem> GetAsync(string id);
        Task<NewsItemExtended[]> GetLatestAsync(int skip, int limit, bool includeUnpublished);
        Task UpdateAsync(NewsItem newsItem);
        Task DeleteAsync(string id);
    }
}
