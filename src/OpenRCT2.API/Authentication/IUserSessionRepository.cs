using System.Threading.Tasks;

namespace OpenRCT2.API.Authentication
{
    public interface IUserSessionRepository
    {
        Task<string> CreateToken(int userId);
        Task<bool> RevokeToken(string token);
        Task<int?> GetUserIdFromToken(string token);
    }
}
