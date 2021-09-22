using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using OpenRCT2.Api.Client;

namespace OpenRCT2.Content.Services
{
    internal class OpenRCT2ApiService
    {
        private readonly AuthorisationService _authService;
        private readonly NavigationManager _navigationManager;
        private readonly Uri _baseAddress = new Uri("http://localhost:5004");
        private readonly string _apiKey = "T4TMKeW4f7w6WFpUnsCw8NNUlUd6wuPiSPZtGQmrsfrgBSEWjDAueGNERIreQlri";
        private OpenRCT2ApiClient _client;

        public OpenRCT2ApiService(AuthorisationService authService, NavigationManager navigationManager)
        {
            _authService = authService;
            _navigationManager = navigationManager;
            _navigationManager.LocationChanged += OnLocationChanged;
            _ = RefreshUser();
        }

        private async void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            await RefreshUser();
        }

        private async Task RefreshUser()
        {
            var client = Client;
            try
            {
                var user = await client.User.Get(_authService.Name);
                if (user == null)
                {
                    await SignOutAsync();
                }
                else
                {
                    await _authService.Update(user);
                }
            }
            catch
            {
            }
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
                await _authService.SetAsync(result.Token, result.Name);
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
