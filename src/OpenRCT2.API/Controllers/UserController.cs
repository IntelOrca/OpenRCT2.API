using System;
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
            var user = await _userRepository.GetUserFromNameAsync(name);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _authService.GetAuthenticatedUserAsync();
            if (CanSeeEntireProfile(currentUser, user))
            {
                return new
                {
                    user.Name,
                    user.Email,
                    user.Status,
                    user.Bio,
                    Joined = user.Created,
                    AvatarUrl = GetAvatarUrl(user),
                    CanEdit = true,
                };
            }
            else
            {
                return new
                {
                    user.Name,
                    user.Bio,
                    Joined = user.Created,
                    AvatarUrl = GetAvatarUrl(user)
                };
            }
        }

        private static bool CanSeeEntireProfile(User currentUser, User user)
        {
            return currentUser.Status == AccountStatus.Administrator || currentUser.Id == user.Id;
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
            [FromServices] UserAccountService userAccountService,
            [FromRoute] string name,
            [FromBody] UpdateUserRequest body)
        {
            var user = await _userRepository.GetUserFromNameAsync(name);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _authService.GetAuthenticatedUserAsync();
            if (!CanSeeEntireProfile(currentUser, user))
            {
                return Unauthorized();
            }

            var isAdmin = currentUser.Status == AccountStatus.Administrator;
            if (body.Name != null && body.Name.ToLowerInvariant() != user.NameNormalised && isAdmin)
            {
                if (await userAccountService.IsNameAvailabilityAsync(body.Name))
                {
                    user.Name = body.Name;
                    user.NameNormalised = body.Name.ToLowerInvariant();
                }
                else
                {
                    _logger.LogInformation($"Failed to update user name for {user.Name}: {body.Name} was taken or invalid");
                    return BadRequest(new
                    {
                        Message = ErrorHandler.GetErrorMessage(ErrorKind.NameAlreadyUsed)
                    });
                }
            }
            if (body.Status != null && isAdmin)
            {
                user.Status = body.Status.Value;
            }
            if (body.EmailCurrent != null)
            {
                user.Email = body.EmailCurrent.ToLowerInvariant();
            }
            if (body.EmailNew != null)
            {
                throw new NotImplementedException();
            }
            if (body.Password != null)
            {
                user.PasswordSalt = Guid.NewGuid().ToString();
                user.PasswordHash = _authService.HashPassword(body.Password, user.PasswordSalt);
            }
            if (body.Bio != null)
            {
                user.Bio = body.Bio;
            }

            _logger.LogInformation($"Updating user: {user.Name}");
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

        [HttpPost("user/recovery")]
        public async Task<object> BeginRecoveryAsync(
            [FromServices] UserAccountService userAccountService,
            [FromBody] UserRecoveryRequest request)
        {
            if (!await _authService.IsClientAuthEnabledAsync())
            {
                // Restrict API to only offical clients
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var isValid = await userAccountService.RequestRecoveryAsync(request.Name);
            if (!isValid)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPut("user/recovery")]
        public async Task<object> CompleteRecoveryAsync(
            [FromServices] UserAccountService userAccountService,
            [FromBody] UserRecoveryRequest request)
        {
            if (!await _authService.IsClientAuthEnabledAsync())
            {
                // Restrict API to only offical clients
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var err = await userAccountService.CompleteRecoveryAsync(request.Token, request.PasswordHash);
            return err switch
            {
                ErrorKind.None => Ok(),
                _ => BadRequest()
            };
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
            var email = user.Email.ToLowerInvariant().Trim();
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
