using System.Threading.Tasks;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Abstractions
{
    public interface IUserRepository
    {
        Task<User> GetByName(string name);
    }
}
