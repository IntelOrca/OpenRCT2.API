using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Services
{
    public class AuthenticationService
    {
        private readonly IAuthTokenRepository _authTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private User _authorizedUser;
        private bool _authorizedUserSet;

        public AuthenticationService(IAuthTokenRepository authTokenRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _authTokenRepository = authTokenRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public ValueTask<bool> IsClientAuthEnabledAsync()
        {
            var req = _httpContextAccessor.HttpContext.Request;
            var apiKey = req.Headers["X-API-KEY"];
            var isEnabled = apiKey == "T4TMKeW4f7w6WFpUnsCw8NNUlUd6wuPiSPZtGQmrsfrgBSEWjDAueGNERIreQlri";
            return ValueTask.FromResult(isEnabled);
        }

        public async ValueTask<User> GetAuthenticatedUserAsync()
        {
            if (!_authorizedUserSet)
            {
                var req = _httpContextAccessor.HttpContext.Request;
                var authorizationHeader = req.Headers[HeaderNames.Authorization].FirstOrDefault();
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    const string BearerPrefix = "Bearer ";
                    if (authorizationHeader.StartsWith(BearerPrefix))
                    {
                        var token = authorizationHeader[BearerPrefix.Length..];
                        var authToken = await _authTokenRepository.GetFromTokenAsync(token);
                        if (authToken != null)
                        {
                            _authorizedUser = await _userRepository.GetUserFromIdAsync(authToken.UserId);
                        }
                        else
                        {
                            _authorizedUser = null;
                        }
                        _authorizedUserSet = true;
                    }
                }
            }
            return _authorizedUser;
        }

        public async ValueTask<User> AuthenticateAsync(string email, string password)
        {
            return null;
        }
    }
}
