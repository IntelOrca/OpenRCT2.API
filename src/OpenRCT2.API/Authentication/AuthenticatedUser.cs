using System.Diagnostics;
using System.Security.Claims;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Authentication
{
    [DebuggerDisplay("{User.Name}")]
    public class AuthenticatedUser : ClaimsIdentity
    {
        public User User { get; }
        public string Token { get; }

        public string Id => User.Id;

        public AuthenticatedUser(User user, string token)
            : base("token")
        {
            User = user;
            Token = token;
        }
    }
}


