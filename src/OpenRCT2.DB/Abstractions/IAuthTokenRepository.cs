using System.Threading.Tasks;
using OpenRCT2.DB.Models;

namespace OpenRCT2.DB.Abstractions
{
    public interface IAuthTokenRepository
    {
        Task<AuthToken> GetFromTokenAsync(string token);
        Task InsertAsync(AuthToken authToken);
        Task UpdateAsync(AuthToken authToken);
        Task DeleteAsync(string token);
        Task DeleteAllAsync(string userId);
    }
}
