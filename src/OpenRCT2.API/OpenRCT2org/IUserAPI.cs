using System.Threading.Tasks;

namespace OpenRCT2.API.OpenRCT2org
{
    public interface IUserApi
    {
        Task<JUser> GetUserAsync(int id);
        Task<JUser> AuthenticateUserAsync(string userName, string password);
    }
}
