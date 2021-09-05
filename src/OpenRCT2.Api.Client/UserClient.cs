using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenRCT2.Api.Client.Models;

namespace OpenRCT2.Api.Client
{
    public class UserClient
    {
        private const string ClientSalt = "openrct2apiclient_";

        private readonly OpenRCT2ApiClient _client;

        internal UserClient(OpenRCT2ApiClient client)
        {
            _client = client;
        }

        public Task Create(string name, string email, string password)
        {
            return _client.PostAsync<object, object, DefaultErrorModel>("user", new {
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password)
            });
        }

        private static string HashPassword(string password)
        {
            var input = ClientSalt + password;
            using (var algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(input));
                return ToHexString(hash);
            }
        }

        private static string ToHexString(byte[] bytes)
        {
            char[] szToken = new char[bytes.Length * 2];
            int szTokenIndex = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                szToken[szTokenIndex++] = ToHex(b >> 4);
                szToken[szTokenIndex++] = ToHex(b & 0x0F);
            }
            return new string(szToken);
        }

        private static char ToHex(int x)
        {
            if (x >= 0)
            {
                if (x <= 9)
                    return (char)('0' + x);
                if (x <= 15)
                    return (char)('a' + x - 10);
            }
            throw new ArgumentOutOfRangeException(nameof(x));
        }
    }
}
