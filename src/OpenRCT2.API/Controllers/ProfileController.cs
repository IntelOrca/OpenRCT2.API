using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Authentication;
using OpenRCT2.API.Implementations;
using OpenRCT2.API.Models.Requests;
using OpenRCT2.API.Services;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Controllers
{
    [Route("profile")]
    public class ProfileController : Controller
    {
        [Authorize]
        [HttpGet]
        public object GetProfile()
        {
            var currentUser = User.Identity as AuthenticatedUser;
            return new
            {
                status = JStatus.OK,
                result = GetProfileInfo(currentUser.User)
            };
        }

        [HttpPost("create")]
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

        [HttpPost("auth")]
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

            return new
            {
                status = JStatus.OK,
                token = authToken.Token,
                user = GetProfileInfo(user)
            };
        }

        [Authorize]
        [HttpDelete("auth")]
        public async Task<object> SignOutAsync(
            [FromServices] UserAuthenticationService userAuthenticationService)
        {
            var currentUser = User.Identity as AuthenticatedUser;
            await userAuthenticationService.RevokeTokenAsync(currentUser.Token);
            return JResponse.OK();
        }

        [HttpGet("verify")]
        public async Task<object> VerifyAsync(
            [FromServices] UserAccountService userAccountService,
            [FromQuery] string token)
        {
            if (await userAccountService.VerifyAccountAsync(token))
            {
                return JResponse.OK();
            }
            else
            {
                return BadRequest(JResponse.Error("Invalid token"));
            }
        }

        [Authorize]
        [HttpPut("verify")]
        public async Task<object> VerifyResetAsync(
            [FromServices] UserAccountService userAccountService)
        {
            var currentUser = User.Identity as AuthenticatedUser;
            await userAccountService.SendVerifyAccountEmailAsync(currentUser.User);
            return JResponse.OK();
        }

        private static object GetProfileInfo(User user)
        {
            return new
            {
                name = user.Name,
                emailVerified = user.EmailVerified != null,
                permissions = new[]
                {
                    "news.write"
                }
            };
        }
    }
}
