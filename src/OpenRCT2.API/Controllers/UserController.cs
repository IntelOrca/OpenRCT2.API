using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Extensions;
using OpenRCT2.API.Models.Requests;
using OpenRCT2.API.Services;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        public const string ErrorUnknownUser = "unknown user";
        public const string ErrorAuthenticationFailed = "authentication failed";

        private readonly UserAuthenticationService _authService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public UserController(UserAuthenticationService authService, IUserRepository userRepository, ILogger<UserController> logger)
        {
            _authService = authService;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("user/{name}")]
        public async Task<object> GetAsync(string name)
        {
            var user = await _userRepository.GetUserFromNameAsync(name.ToLower());
            if (user == null)
            {
                return NotFound();
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

        [HttpPost("user")]
        public async Task<object> PostAsync(
            [FromServices] UserAccountService userAccountService,
            [FromBody] CreateUserRequest body)
        {
            if (!await _authService.IsClientAuthEnabledAsync())
            {
                // Restrict API to only offical clients
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if ((body.Name ?? "").Length <= 3)
            {
                return BadRequest();
            }
            if ((body.Email ?? "").Count(c => c == '@') != 1)
            {
                return BadRequest();
            }
            if ((body.PasswordHash ?? "").Length != 64)
            {
                return BadRequest();
            }

            // var remoteIp = HttpContext.Connection.RemoteIpAddress.ToString();
            // if (!await recaptchaService.ValidateAsync(remoteIp, body.Captcha).ConfigureAwait(false))
            // {
            //     return BadRequest(JResponse.Error("reCAPTCHA validation failed."));
            // }
            if (!await userAccountService.IsNameAvailabilityAsync(body.Name))
            {
                return BadRequest(new {
                    Reason = "User name already taken."
                });
            }
            if (!await userAccountService.IsEmailAvailabilityAsync(body.Email))
            {
                return BadRequest(new
                {
                    Reason = "Email address already registered."
                });
            }
            await userAccountService.CreateAccountAsync(body.Name, body.Email, body.PasswordHash);
            return Ok();
        }

        [HttpPut("user/{name}")]
        public async Task<object> PutAsync(
            [FromRoute] string name,
            [FromBody] UpdateUserRequest body)
        {
            var user = await _userRepository.GetUserFromNameAsync(name.ToLower());
            if (user == null)
            {
                return NotFound();
            }

            _logger.LogInformation($"Updating user: {user.Name}");

            if (body.Bio != null)
            {
                user.Bio = body.Bio;
            }
            await _userRepository.UpdateUserAsync(user);
            return Ok();
        }

        [HttpPost("user/verify")]
        public async Task<object> VerifyAsync(
            [FromServices] UserAccountService userAccountService,
            [FromBody] UserVerifyRequest body)
        {
            if (!await _authService.IsClientAuthEnabledAsync())
            {
                // Restrict API to only offical clients
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (await userAccountService.VerifyAccountAsync(body.Token))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("users")]
        public async Task<object> GetAllAsync(
            [FromServices] IUserRepository userRepository)
        {
            var user = await _authService.GetAuthenticatedUserAsync();
            if (user.Status != AccountStatus.Administrator)
            {
                return Unauthorized();
            }

            var users = await userRepository.GetAllAsync();
            return new {
                Users = users
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
    }
}
