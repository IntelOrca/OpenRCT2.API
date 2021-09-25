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

        public Task<UserModel[]> GetAll(UserQueryRequest request)
        {
            var url = _client.UrlEncode("user?page={0}&pageSize={1}", request.Page, request.PageSize);
            return _client.GetAsync<UserModel[]>(url);
        }

        public Task<UserModel> Get(string name)
        {
            var url = _client.UrlEncode("user/{0}", name);
            return _client.GetAsync<UserModel>(url);
        }

        public Task Create(string name, string email, string password)
        {
            return _client.PostAsync<object, object, DefaultErrorModel>("user", new {
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password)
            });
        }

        public Task Verify(string token)
        {
            return _client.PostAsync<object>("user/verify", new
            {
                Token = token
            });
        }

        public Task RequestRecovery(string nameOrEmail)
        {
            return _client.PostAsync<object>("user/recovery", new
            {
                Name = nameOrEmail
            });
        }

        public Task CompleteRecovery(string token, string password)
        {
            return _client.PutAsync<object>("user/recovery", new
            {
                Token = token,
                PasswordHash = HashPassword(password)
            });
        }

        public Task Edit(string name, UserEditRequest request)
        {
            var url = _client.UrlEncode("user/{0}", name);
            return _client.PutAsync<object, object, DefaultErrorModel>(url, new
            {
                request.Name,
                request.Status,
                request.SuspensionReason,
                request.EmailCurrent,
                request.EmailNew,
                PasswordHash = string.IsNullOrEmpty(request.Password) ? null : HashPassword(request.Password),
                request.Bio,
            });
        }

        public Task<string> GenerateSecretKey(string name)
        {
            var url = _client.UrlEncode($"user/generateSecret");
            return _client.PostAsync<string>(url, new
            {
                Name = name
            });
        }

        internal static string HashPassword(string password)
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
