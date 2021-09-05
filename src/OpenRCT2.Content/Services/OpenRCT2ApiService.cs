using System;
using System.Net;
using System.Threading.Tasks;
using OpenRCT2.Api.Client;

namespace OpenRCT2.Content.Services
{
    internal class OpenRCT2ApiService
    {
        private readonly AuthorisationService _authService;
        private readonly Uri _baseAddress = new Uri("http://localhost:5002");
        private readonly string _apiKey = "T4TMKeW4f7w6WFpUnsCw8NNUlUd6wuPiSPZtGQmrsfrgBSEWjDAueGNERIreQlri";
        private OpenRCT2ApiClient _client;

        public OpenRCT2ApiService(AuthorisationService authService)
        {
            _authService = authService;
        }

        private void RefreshClient()
        {
            if (string.IsNullOrEmpty(_authService.Token))
            {
                _client = new OpenRCT2ApiClient(_baseAddress, apiKey: _apiKey);
            }
            else
            {
                _client = new OpenRCT2ApiClient(_baseAddress, _authService.Token, _apiKey);
            }
        }

        public async Task<bool> SignInAsync(string email, string pass)
        {
            try
            {
                var result = await Client.Auth.AuthenticateAsync(email, pass);
                await _authService.SetAsync(result.Token, result.FullName);
                return true;
            }
            catch (OpenRCT2ApiClientStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return false;
            }
        }

        public async Task<bool> SignOutAsync()
        {
            if (_authService.IsSignedIn)
            {
                await Client.Auth.RevokeTokenAsync(_authService.Token);
                await _authService.ClearAsync();
            }
            return true;
        }

        public OpenRCT2ApiClient Client
        {
            get
            {
                if (_client == null || _client.AuthorizationToken != _authService.Token)
                {
                    RefreshClient();
                }
                return _client;
            }
        }
    }
}
