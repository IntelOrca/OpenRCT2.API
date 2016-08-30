using System.Threading.Tasks;

namespace OpenRCT2.API.Abstractions
{
    public interface IUserSessionRepository
    {
        Task<string> CreateToken(int userId);
        Task<bool> RevokeToken(string token);
        Task<int?> GetUserIdFromToken(string token);
    }
}
