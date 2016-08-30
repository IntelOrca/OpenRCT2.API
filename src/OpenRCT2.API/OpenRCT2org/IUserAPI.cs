using System.Threading.Tasks;

namespace OpenRCT2.API.OpenRCT2org
{
    public interface IUserApi
    {
        Task<JUser> GetUser(int id);
        Task<JUser> AuthenticateUser(string userName, string password);
    }
}
