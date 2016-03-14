using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Extensions;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users = new List<User>();
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly Random _random;

        private void CreateUser(string name, string password)
        {
            var user = new User();
            user.Name = name;
            user.AuthenticationSalt = _random.NextBytes(16)
                                             .ToHexString();
            user.PasswordHash = _userAuthenticator.GetPasswordHash(user, password);
            _users.Add(user);
        }

        public UserRepository(IUserAuthenticator userAuthenticator, Random random)
        {
            _userAuthenticator = userAuthenticator;
            _random = random;

            CreateUser("IntelOrca", "donkey");
            CreateUser("GACJ", "chicken");
        }

        public Task<User> GetByName(string name)
        {
            User user = _users.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }
    }
}
