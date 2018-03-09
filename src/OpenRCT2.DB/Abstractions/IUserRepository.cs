using System.Threading.Tasks;
using OpenRCT2.DB.Models;

namespace OpenRCT2.DB.Abstractions
{
    public interface IUserRepository
    {
        Task<User[]> GetAllAsync();
        Task<User> GetUserFromIdAsync(string id);
        Task<User> GetUserFromNameAsync(string name);
        Task<User> GetUserFromEmailAsync(string email);
        Task<User> GetUserFromOpenRCT2orgIdAsync(int id);
        Task<User> GetFromEmailVerifyTokenAsync(string token);
        Task InsertUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}
