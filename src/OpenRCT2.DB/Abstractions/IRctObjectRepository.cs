using System.Threading.Tasks;
using OpenRCT2.DB.Models;

namespace OpenRCT2.DB.Abstractions
{
    public interface IRctObjectRepository
    {
        Task<RctObject[]> GetAllAsync();
        Task<RctObject[]> ByNameAsync(string name);
        Task<RctObject[]> ByHeaderAsync(string header);
    }
}
