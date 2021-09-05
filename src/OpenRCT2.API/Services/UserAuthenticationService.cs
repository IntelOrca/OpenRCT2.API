using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OpenRCT2.API.Configuration;
using OpenRCT2.API.Extensions;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Services
{
    public class UserAuthenticationService
    {
        private readonly ApiConfig _config;
        private readonly IAuthTokenRepository _authTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private User _authorizedUser;
        private bool _authorizedUserSet;

        public UserAuthenticationService(
            IOptions<ApiConfig> config,
            IAuthTokenRepository authTokenRepository,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserAuthenticationService> logger)
        {
            _config = config.Value;
            _authTokenRepository = authTokenRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
            _logger.LogInformation($"Authenticating user with email / name: '{email}'");

            User user;
            if (email != null && email.Contains('@'))
            {
                user = await _userRepository.GetUserFromEmailAsync(email);
            }
            else
            {
                user = await _userRepository.GetUserFromNameAsync(email);
            }

            if (user != null)
            {
                var givenHash = HashPassword(password, user.PasswordSalt);
                if (givenHash == user.PasswordHash)
                {
                    return user;
                }
                else
                {
                    _logger.LogInformation($"Authentication failed (wrong password) for user with email / name: '{email}'");
                }
            }
            else
            {
                _logger.LogInformation($"Authentication failed, no such email / name: '{email}'");
            }
            return null;
        }

        public string HashPassword(string passwordHash, string salt)
        {
            var serverSalt = _config.PasswordServerSalt;
            var input = serverSalt + passwordHash + salt;
            using (var algorithm = SHA512.Create())
            {
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(input));
                return hash.ToHexString();
            }
        }
    }
}
