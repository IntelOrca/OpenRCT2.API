using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Extensions
{
    public static class UserRepositoryExtensions
    {
        public static Task<User> GetUserFromEmailOrNameAsync(this IUserRepository userRepository, string emailOrName)
        {
            if (emailOrName != null && emailOrName.Contains('@'))
            {
                return userRepository.GetUserFromEmailAsync(emailOrName);
            }
            else
            {
                return userRepository.GetUserFromNameAsync(emailOrName);
            }
        }
    }
}
