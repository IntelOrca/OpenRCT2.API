using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRCT2.API.Configuration;
using OpenRCT2.API.Controllers;
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
            if (name == null || name.Length < 3)
            {
                return false;
            }

            if (_reservedUserNames.Contains(name))
            {
                return false;
            }

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
                NameNormalised = name.ToLowerInvariant(),
                Email = email.ToLowerInvariant(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Created = utcNow,
                Modified = utcNow,
                EmailVerifyToken = GenerateToken256()
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
                user.EmailVerifyToken = GenerateToken256();
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

                    var now = DateTime.UtcNow;
                    user.EmailVerifyToken = null;
                    user.EmailVerified = now;
                    if (await IsFirstUserAsync())
                    {
                        user.Status = AccountStatus.Administrator;
                    }
                    else
                    {
                        user.Status = AccountStatus.Active;
                    }
                    user.Modified = now;

                    await _userRepository.UpdateUserAsync(user);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> RequestRecoveryAsync(string emailOrName)
        {
            var user = await _userRepository.GetUserFromEmailOrNameAsync(emailOrName);
            if (user == null)
            {
                return false;
            }

            user.RecoveryToken = GenerateToken256();
            await _userRepository.UpdateUserAsync(user);

            var recoveryLink = GetResetPasswordLink(user);
            await _emailer.Email
                .To(user.Email)
                .Subject("OpenRCT2.io - Account Recovery")
                .Body(
                    $"Hello {user.Name},\n\n" +
                    $"We received an account recovery request for {user.Email}.\n\n" +
                    $"Click on the link below or copy it into your web browser to reset your password.\n" +
                    $"{recoveryLink}\n\n" +
                    $"If you did not make this request, then you can ignore this email.\n\n" +
                    $"OpenRCT2 Team")
                .SendAsync();

            return true;
        }

        public async Task<ErrorKind> CompleteRecoveryAsync(string token, string password)
        {
            _logger.LogInformation($"Completing recovery for user account");

            var user = await _userRepository.GetFromRecoveryTokenAsync(token);
            if (user == null)
            {
                _logger.LogInformation($"Recovery for user failed, no user matched token");
                return ErrorKind.InvalidToken;
            }

            var now = DateTime.UtcNow;

            if (user.Status == AccountStatus.NotVerified)
            {
                // Allow user to to be verified via account recovery (since it proves a valid email address)
                _logger.LogInformation($"Account verified (via recovery): {user.Name}");
                if (await IsFirstUserAsync())
                {
                    user.Status = AccountStatus.Administrator;
                }
                else
                {
                    user.Status = AccountStatus.Active;
                }
                user.EmailVerified = now;
            }

            user.RecoveryToken = null;
            user.PasswordSalt = Guid.NewGuid().ToString();
            user.PasswordHash = _userAuthenticationService.HashPassword(password, user.PasswordSalt);
            user.Modified = now;

            await _userRepository.UpdateUserAsync(user);
            _logger.LogInformation($"Recovery for user {user.Name} successful");
            return ErrorKind.None;
        }

        public async Task<string> GenerateSecretKeyAsync(User user)
        {
            var secret = GenerateSecretKey();
            user.SecretKey = secret;
            await _userRepository.UpdateUserAsync(user);
            _logger.LogInformation($"New secret key generated for user {user.Name}");
            return secret;
        }

        private static string GenerateSecretKey()
        {
            var token = GenerateToken256();
            return $"{token[0..2]}-{token[2..4]}-{token[4..6]}-{token[6..8]}";
        }

        private async Task<bool> IsFirstUserAsync()
        {
            return (await _userRepository.GetUserCountAsync()) <= 1;
        }

        private static string GenerateToken256()
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

        private static string GetResetPasswordLink(User user)
        {
            return $"https://openrct2.io/recovery?token={user.RecoveryToken}";
        }

        private readonly HashSet<string> _reservedUserNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "rct",
            "rct1",
            "rct2",
            "rct3",
            "rct4",
            "openrct2",
            "openloco",
            "loco",
            "locomotion",

            "about",
            "admin",
            "api",
            "author",
            "contact",
            "content",
            "index",
            "privacy",
            "popular",
            "recent",
            "recovery",
            "signin",
            "signout",
            "signup",
            "terms",
            "trending",
            "user",
            "verify"
        };
    }
}
