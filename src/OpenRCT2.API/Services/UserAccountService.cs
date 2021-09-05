using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRCT2.API.Configuration;
using OpenRCT2.API.Extensions;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Services
{
    public class UserAccountService
    {
        private readonly ApiConfig _config;
        private readonly UserAuthenticationService _userAuthenticationService;
        private readonly IUserRepository _userRepository;
        private readonly Emailer _emailer;
        private readonly ILogger _logger;

        public UserAccountService(
            IOptions<ApiConfig> config,
            UserAuthenticationService userAuthenticationService,
            IUserRepository userRepository,
            Emailer emailer,
            ILogger<UserAccountService> logger)
        {
            _config = config.Value;
            _userAuthenticationService = userAuthenticationService;
            _userRepository = userRepository;
            _emailer = emailer;
            _logger = logger;
        }

        public async Task<bool> IsNameAvailabilityAsync(string name)
        {
            var user = await _userRepository.GetUserFromNameAsync(name);
            return user == null;
        }

        public async Task<bool> IsEmailAvailabilityAsync(string email)
        {
            var user = await _userRepository.GetUserFromEmailAsync(email);
            return user == null;
        }

        public async Task<User> CreateAccountAsync(string name, string email, string password)
        {
            _logger.LogInformation($"Creating user account: {name} <{email}>");
            var passwordSalt = Guid.NewGuid().ToString();
            var passwordHash = _userAuthenticationService.HashPassword(password, passwordSalt);
            var utcNow = DateTime.UtcNow;
            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                NameNormalised = name.ToLower(),
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Created = utcNow,
                Modified = utcNow,
                EmailVerifyToken = GenerateEmailConfirmToken()
            };
            await _userRepository.InsertUserAsync(user);
            _logger.LogInformation($"User {user.Id} created");
            await SendVerifyAccountEmailAsync(user);
            return user;
        }

        public async Task SendVerifyAccountEmailAsync(User user)
        {
            // Reset token if null
            if (user.EmailVerifyToken == null)
            {
                // Refresh user object before send to database
                user = await _userRepository.GetUserFromIdAsync(user.Id);
                user.EmailVerifyToken = GenerateEmailConfirmToken();
                await _userRepository.UpdateUserAsync(user);
            }

            try
            {
                var emailConfirmLink = GetEmailConfirmLink(user);
                await _emailer.Email
                    .To(user.Email)
                    .Subject("OpenRCT2.io - Account verification")
                    .Body(
                        $"Hello {user.Name},\n\n" +
                        $"Please confirm your email address by clicking on the link below.\n\n" +
                        $"{emailConfirmLink}\n\n" +
                        $"If you did not sign up to OpenRCT2.io then you can ignore this email.\n\n" +
                        $"OpenRCT2 Team")
                    .SendAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send account verification email.");
            }
        }

        public async Task<bool> VerifyAccountAsync(string token)
        {
            _logger.LogInformation($"Verifying account with token: {token}");
            var user = await _userRepository.GetFromEmailVerifyTokenAsync(token);
            if (user == null)
            {
                return false;
            }
            else
            {
                if (user.Status == AccountStatus.NotVerified)
                {
                    _logger.LogInformation($"Account verified: {user.Name}");
                    user.EmailVerifyToken = null;
                    user.EmailVerified = DateTime.UtcNow;
                    user.Status = AccountStatus.Active;
                    await _userRepository.UpdateUserAsync(user);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static string GenerateEmailConfirmToken()
        {
            using (var algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(Guid.NewGuid().ToByteArray());
                return hash.ToHexString();
            }
        }

        private static string GetEmailConfirmLink(User user)
        {
            return $"https://openrct2.io/verify?token={user.EmailVerifyToken}";
        }
    }
}
