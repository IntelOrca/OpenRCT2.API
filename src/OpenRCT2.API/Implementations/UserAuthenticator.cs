using System;
using System.Security.Cryptography;
using System.Text;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Extensions;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Implementations
{
    public class UserAuthenticator : IUserAuthenticator
    {
        const int TokenSize = 16;

        public bool CheckPassword(User user, string password)
        {
            string hash = GetPasswordHash(user, password);
            return user.PasswordHash == hash;
        }

        public string GetPasswordHash(User user, string password)
        {
            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            byte[] userSaltBytes = Encoding.ASCII.GetBytes(user.AuthenticationSalt);

            int minLength = Math.Min(passwordBytes.Length, userSaltBytes.Length);
            int maxLength = Math.Max(passwordBytes.Length, userSaltBytes.Length);

            byte[] input = new byte[maxLength];
            Array.Copy(passwordBytes, input, passwordBytes.Length);
            for (int i = 0; i < userSaltBytes.Length; i++)
            {
                input[i] = (byte)(input[i] ^ userSaltBytes[i]);
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] key = sha256.ComputeHash(input);
                return key.ToHexString();
            }
        }

        public string GetAuthenticationKey(User user, string token)
        {
            byte[] input = Encoding.ASCII.GetBytes(token);
            byte[] userSalt = Encoding.ASCII.GetBytes(user.AuthenticationSalt);
            int minLength = Math.Min(input.Length, userSalt.Length);
            for (int i = 0; i < minLength; i++)
            {
                input[i] = (byte)(input[i] ^ userSalt[i]);
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] key = sha256.ComputeHash(input);
                return key.ToHexString();
            }
        }

        public string GenerateAuthenticationToken(Random random)
        {
            byte[] bytes = new byte[TokenSize];
            random.NextBytes(bytes);
            return bytes.ToHexString();
        }
    }
}
