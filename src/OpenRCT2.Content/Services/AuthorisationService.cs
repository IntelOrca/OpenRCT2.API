using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using OpenRCT2.Api.Client.Models;

namespace OpenRCT2.Content.Services
{
    internal class AuthorisationService
    {
        private const string STOREAGE_KEY_TOKEN = "auth_token";
        private const string STOREAGE_KEY_NAME = "auth_name";
        private const string STOREAGE_KEY_STATUS = "auth_status";

        private readonly ILocalStorageService _localStorage;

        public event EventHandler OnAuthorisationChanged;

        public string Name { get; private set; }
        public string Token { get; private set; }
        public UserAccountStatus Status { get; private set; }
        public string SuspensionReason { get; private set; }

        public bool IsSignedIn => !string.IsNullOrEmpty(Token);
        public bool IsGuest => !IsSignedIn;
        public bool IsPower =>
            Status != UserAccountStatus.NotVerified &&
            Status != UserAccountStatus.Suspended;
        public bool IsAdmin => Status == UserAccountStatus.Administrator;

        public AuthorisationService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SetFromLocalStorageAsync()
        {
            Token = await _localStorage.GetItemAsStringAsync(STOREAGE_KEY_TOKEN);
            Name = await _localStorage.GetItemAsStringAsync(STOREAGE_KEY_NAME);
            Status = await _localStorage.GetItemAsync<UserAccountStatus>(STOREAGE_KEY_STATUS);
        }

        public async Task ClearAsync()
        {
            Token = null;
            Name = null;

            await _localStorage.RemoveItemAsync(STOREAGE_KEY_TOKEN);
            await _localStorage.RemoveItemAsync(STOREAGE_KEY_NAME);
            await _localStorage.RemoveItemAsync(STOREAGE_KEY_STATUS);

            RaiseOnAuthorisationChanged();
        }

        public async Task SetAsync(string token, string name)
        {
            Token = token;
            Name = name;

            await _localStorage.SetItemAsStringAsync(STOREAGE_KEY_TOKEN, Token);
            await _localStorage.SetItemAsStringAsync(STOREAGE_KEY_NAME, Name);

            RaiseOnAuthorisationChanged();
        }

        public async Task Update(UserModel user)
        {
            Name = user.Name;
            Status = user.Status;
            SuspensionReason = user.SuspensionReason;

            await _localStorage.SetItemAsStringAsync(STOREAGE_KEY_NAME, Name);
            await _localStorage.SetItemAsync(STOREAGE_KEY_STATUS, Status);

            RaiseOnAuthorisationChanged();
        }

        private void RaiseOnAuthorisationChanged() => OnAuthorisationChanged?.Invoke(this, EventArgs.Empty);
    }
}
