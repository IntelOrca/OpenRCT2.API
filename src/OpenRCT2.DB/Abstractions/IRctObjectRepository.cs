using OpenRCT2.DB.Models;
using System.Threading.Tasks;

namespace OpenRCT2.DB.Abstractions
{
    public interface IRctObjectRepository
    {
        Task<LegacyRctObject> GetLegacyFromNameAsync(string name);
        Task<LegacyRctObject> GetLegacyObjectWithHighestNeIdAsync();
        Task UpdateLegacyAsync(LegacyRctObject legacyRctObject);
    }
}
