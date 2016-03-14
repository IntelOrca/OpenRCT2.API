using System;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Abstractions
{
    public interface IUserAuthenticator
    {
        bool CheckPassword(User user, string password);
        string GetPasswordHash(User user, string password);
        string GetAuthenticationKey(User user, string token);
        string GenerateAuthenticationToken(Random random);
    }
}
