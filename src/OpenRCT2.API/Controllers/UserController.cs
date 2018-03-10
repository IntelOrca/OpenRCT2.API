﻿using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.ActionFilters;
using OpenRCT2.API.Authentication;
using OpenRCT2.API.Extensions;
using OpenRCT2.API.Implementations;
using OpenRCT2.API.Models.Requests;
using OpenRCT2.API.Services;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Controllers
{
    [ValidateModelState]
    public class UserController : Controller
    {
        public const string ErrorUnknownUser = "unknown user";
        public const string ErrorAuthenticationFailed = "authentication failed";

        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        #region Request / Response Models

        public class JGetAuthSessionRequest
        {
            public string user { get; set; }
            public string password { get; set; }
        }

        public class JGetAuthSessionResponse : JResponse
        {
            public string user { get; set; }
            public string session { get; set; }
        }

        public class JGetAuthTokenRequest
        {
            public string user { get; set; }
        }

        public class JGetAuthTokenResponse : JResponse
        {
            public string user { get; set; }
            public string token { get; set; }
            public string key { get; set; }
        }

        public class JGetAuthKeyRequest
        {
            public string user { get; set; }
            public string password { get; set; }
            public string token { get; set; }
        }

        public class JGetAuthKeyResponse : JResponse
        {
            public string key { get; set; }
        }

        public class JLoginRequest
        {
            public string user { get; set; }
            public string password { get; set; }
        }

        public class JLoginResponse : JResponse
        {
            public string user { get; set; }
            public string token { get; set; }
        }

        public class JLogoutRequest
        {
            public string token { get; set; }
        }

        public class JProfileUpdateRequest
        {
            public string Bio { get; set; }
        }

        #endregion

        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("user/{name}")]
        public async Task<object> GetAsync(string name)
        {
            var user = await _userRepository.GetUserFromNameAsync(name.ToLower());
            if (user == null)
            {
                return NotFound(JResponse.Error("User not found"));
            }
            return new
            {
                Status = JStatus.OK,
                Result = new
                {
                    user.Name,
                    user.Bio,
                    Joined = user.Created.ToString("d MMMM yyyy"),
                    Comments = 0,
                    Uploads = 0,
                    Traits = new [] { "Developer", "Streamer" },
                    Avatar = GetAvatarUrl(user)
                }
            };
        }

        [HttpPut("user/{name}")]
        public async Task<object> PutAsync(
            [FromRoute] string name,
            [FromBody] UpdateUserRequest body)
        {
            var user = await _userRepository.GetUserFromNameAsync(name.ToLower());
            if (user == null)
            {
                return NotFound(JResponse.Error("User not found"));
            }

            _logger.LogInformation($"Updating user: {user.Name}");

            if (body.Bio != null)
            {
                user.Bio = body.Bio;
            }
            await _userRepository.UpdateUserAsync(user);
            return JResponse.OK();
        }

        [HttpPost("user/create")]
        public async Task<object> CreateAsync(
            [FromServices] GoogleRecaptchaService recaptchaService,
            [FromServices] UserAccountService userAccountService,
            [FromBody] CreateUserRequest body)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress.ToString();
            if (!await recaptchaService.ValidateAsync(remoteIp, body.Captcha).ConfigureAwait(false))
            {
                return BadRequest(JResponse.Error("reCAPTCHA validation failed."));
            }
            if (!await userAccountService.IsNameAvailabilityAsync(body.Username))
            {
                return BadRequest(JResponse.Error("User name already taken."));
            }
            if (!await userAccountService.IsEmailAvailabilityAsync(body.Email))
            {
                return BadRequest(JResponse.Error("Email address already registered."));
            }
            await userAccountService.CreateAccountAsync(body.Username, body.Email, body.Password);
            return JResponse.OK();
        }

        [HttpPost("user/auth")]
        public async Task<object> SignInAsync(
            [FromServices] UserAuthenticationService userAuthenticationService,
            [FromBody] SignInRequest body)
        {
            var (user, authToken) = await userAuthenticationService.AuthenticateAsync(body.Username, body.Password);
            if (authToken == null)
            {
                var result = JResponse.Error("Invalid username or password.");
                return StatusCode(StatusCodes.Status401Unauthorized, result);
            }

            return new {
                status = JStatus.OK,
                token = authToken.Token,
                user = GetProfileInfo(user)
            };
        }

        [Authorize]
        [HttpGet("user/profile")]
        public object GetProfileAsync()
        {
            var currentUser = User.Identity as AuthenticatedUser;
            return new
            {
                status = JStatus.OK,
                result = GetProfileInfo(currentUser.User)
            };
        }

        [Authorize]
        [HttpDelete("user/auth")]
        public async Task<object> SignOutAsync(
            [FromServices] UserAuthenticationService userAuthenticationService)
        {
            var currentUser = User.Identity as AuthenticatedUser;
            await userAuthenticationService.RevokeTokenAsync(currentUser.Token);
            return JResponse.OK();
        }

        [HttpGet("users")]
        [Authorize(Roles = "Administrator")]
        public async Task<object> GetAllAsync(
            [FromServices] IUserRepository userRepository)
        {
            var users = await userRepository.GetAllAsync();
            return new
            {
                status = JStatus.OK,
                users = users
            };
        }

        private static string GetAvatarUrl(User user)
        {
            var email = user.Email.ToLower().Trim();
            var emailBytes = Encoding.ASCII.GetBytes(email);
            string emailMd5;
            using (var md5 = MD5.Create())
            {
                emailMd5 = md5.ComputeHash(emailBytes).ToHexString();
            }
            return $"https://www.gravatar.com/avatar/{emailMd5}";
        }

        private static object GetProfileInfo(User user)
        {
            return new
            {
                name = user.Name,
                permissions = new[]
                {
                    "news.write"
                }
            };
        }
    }
}
