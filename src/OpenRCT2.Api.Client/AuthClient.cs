using System.Threading.Tasks;
using OpenRCT2.Api.Client.Models;

namespace OpenRCT2.Api.Client
{
    public class AuthClient
    {
        private readonly OpenRCT2ApiClient _client;

        internal AuthClient(OpenRCT2ApiClient client)
        {
            _client = client;
        }

        public Task<AuthenticationResultModel> AuthenticateAsync(string email, string password)
        {
            return _client.PostAsync<AuthenticationResultModel>("auth", new {
                Email = email,
                Password = UserClient.HashPassword(password)
            });
        }

        public Task RevokeTokenAsync(string token)
        {
            return _client.DeleteAsync<object>("auth", new { token });
        }
    }
}
