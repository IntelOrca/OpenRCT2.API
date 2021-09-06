using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace OpenRCT2.Content.Services
{
    internal class AuthorisationService
    {
        private readonly ILocalStorageService _localStorage;

        public event EventHandler OnAuthorisationChanged;

        public string Name { get; private set; }
        public string Token { get; private set; }

        public bool IsSignedIn => !string.IsNullOrEmpty(Token);
        public bool IsGuest => !IsSignedIn;
        public bool IsPower => true;
        public bool IsAdmin => false;

        public AuthorisationService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SetFromLocalStorageAsync()
        {
            Token = await _localStorage.GetItemAsStringAsync("auth_token");
            Name = await _localStorage.GetItemAsStringAsync("auth_name");
        }

        public async Task ClearAsync()
        {
            Token = null;
            Name = null;

            await _localStorage.RemoveItemAsync("auth_token");
            await _localStorage.RemoveItemAsync("auth_name");

            OnAuthorisationChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task SetAsync(string token, string name)
        {
            Token = token;
            Name = name;

            await _localStorage.SetItemAsStringAsync("auth_token", Token);
            await _localStorage.SetItemAsStringAsync("auth_name", Name);

            OnAuthorisationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
