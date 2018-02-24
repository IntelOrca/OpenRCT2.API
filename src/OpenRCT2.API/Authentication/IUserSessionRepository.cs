using System.Threading.Tasks;

namespace OpenRCT2.API.Authentication
{
    public interface IUserSessionRepository
    {
        Task<string> CreateTokenAsync(int userId);
        Task<bool> RevokeTokenAsync(string token);
        Task<int?> GetUserIdFromTokenAsync(string token);
    }
}
