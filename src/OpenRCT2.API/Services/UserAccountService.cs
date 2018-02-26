using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRCT2.API.Extensions;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Services
{
    public class UserAccountService
    {
        private readonly UserAuthenticationService _userAuthenticationService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public UserAccountService(
            UserAuthenticationService userAuthenticationService,
            IUserRepository userRepository,
            ILogger<UserAccountService> logger)
        {
            _userAuthenticationService = userAuthenticationService;
            _userRepository = userRepository;
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
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Created = utcNow,
                Modified = utcNow
            };
            await _userRepository.InsertUserAsync(user);
            _logger.LogInformation($"User {user.Id} created");
            return user;
        }
    }
}
