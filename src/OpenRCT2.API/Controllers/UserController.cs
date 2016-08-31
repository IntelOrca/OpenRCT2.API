using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Diagnostics;
using OpenRCT2.API.Extensions;
using OpenRCT2.API.Implementations;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Controllers
{
    public class UserController : Controller
    {
        public const string ErrorUnknownUser = "unknown user";
        public const string ErrorAuthenticationFailed = "authentication failed";

        private readonly ILogger<UserController> _logger;

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

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost("user/login")]
        public async Task<IJResponse> Login(
            [FromServices] OpenRCT2org.IUserApi userApi,
            [FromServices] IUserSessionRepository userSessionRepository,
            [FromBody] JLoginRequest body)
        {
            try
            {
                Guard.ArgumentNotNull(body);
                Guard.ArgumentNotNull(body.user);
                Guard.ArgumentNotNull(body.password);
            }
            catch
            {
                return JResponse.Error(JErrorMessages.InvalidRequest);
            }

            _logger.LogInformation("User login: {0}", body.user);

            OpenRCT2org.JUser orgUser;
            try
            {
                orgUser = await userApi.AuthenticateUser(body.user, body.password);
            }
            catch (OpenRCT2org.OpenRCT2orgException)
            {
                return JResponse.Error(ErrorAuthenticationFailed);
            }

            string token = await userSessionRepository.CreateToken(orgUser.userId);
            return new JLoginResponse()
            {
                status = JStatus.OK,
                user = orgUser.name,
                token = token
            };
        }

        [HttpPost("user/logout")]
        public async Task<IJResponse> Logout(
            [FromServices] IUserSessionRepository userSessionRepository,
            [FromBody] JLogoutRequest body)
        {
            try
            {
                Guard.ArgumentNotNull(body);
                Guard.ArgumentNotNull(body.token);
            }
            catch
            {
                return JResponse.Error(JErrorMessages.InvalidRequest);
            }

            if (await userSessionRepository.RevokeToken(body.token))
            {
                return JResponse.OK();
            }
            else
            {
                return JResponse.Error(JErrorMessages.InvalidToken);
            }
        }

        [HttpPut("user/profile")]
        public async Task<IJResponse> ProfileUpdate(
            [FromServices] IUserSessionRepository userSessionRepository,
            [FromBody] JProfileUpdateRequest body)
        {
            string token = GetAuthorizationToken();
            if (token == null)
            {
                return JResponse.Error(JErrorMessages.InvalidToken);
            }

            int? userId = await userSessionRepository.GetUserIdFromToken(token);
            if (!userId.HasValue)
            {
                return JResponse.Error(JErrorMessages.InvalidToken);
            }

            return JResponse.OK();
        }

        private string GetAuthorizationToken()
        {
            string authorization = HttpContext.Request.Headers[HeaderNames.Authorization];
            string[] authorizationParts = authorization.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (authorizationParts.Length >= 2 && authorizationParts[0] == "Bearer")
            {
                string token = authorizationParts[1];
                return token;
            }
            return null;
        }

        #region Old

        [HttpPost("user/getauthsession")]
        public async Task<IJResponse> GetAuthenticationSession(
            [FromServices] Random random,
            [FromServices] OpenRCT2org.IUserApi userApi,
            [FromServices] IUserRepository userRepository,
            [FromServices] IUserAuthenticator userAuthenticator,
            [FromBody] JGetAuthSessionRequest body)
        {
            try
            {
                Guard.ArgumentNotNull(body);
                Guard.ArgumentNotNull(body.user);
                Guard.ArgumentNotNull(body.password);
            }
            catch
            {
                return JResponse.Error(JErrorMessages.InvalidRequest);
            }

            OpenRCT2org.JUser orgUser;
            try
            {
                orgUser = await userApi.AuthenticateUser(body.user, body.password);
            }
            catch (OpenRCT2org.OpenRCT2orgException)
            {
                return JResponse.Error(ErrorAuthenticationFailed);
            }

            return new JGetAuthSessionResponse()
            {
                status = JStatus.OK,
                user = orgUser.name,
                session = random.NextBytes(16).ToHexString()
            };
        }

        [HttpPost("user/getauthtoken")]
        public async Task<IJResponse> GetAuthenticationToken(
            [FromServices] Random random,
            [FromServices] IUserRepository userRepository,
            [FromServices] IUserAuthenticator userAuthenticator,
            [FromBody] JGetAuthTokenRequest body)
        {
            try
            {
                Guard.ArgumentNotNull(body);
                Guard.ArgumentNotNull(body.user);
            }
            catch
            {
                return JResponse.Error(JErrorMessages.InvalidRequest);
            }

            User user = await userRepository.GetByName(body.user);
            if (user == null)
            {
                return JResponse.Error(ErrorUnknownUser);
            }

            string token = userAuthenticator.GenerateAuthenticationToken(random);
            string key = userAuthenticator.GetAuthenticationKey(user, token);

            return new JGetAuthTokenResponse()
            {
                status = JStatus.OK,
                user = user.Name,
                token = token,
                key = key
            };
        }

        [HttpPost("user/getauthkey")]
        public async Task<IJResponse> GetAuthenticationKey(
            [FromServices] IUserRepository userRepository,
            [FromServices] IUserAuthenticator userAuthenticator,
            [FromBody] JGetAuthKeyRequest body)
        {
            try
            {
                Guard.ArgumentNotNull(body);
                Guard.ArgumentNotNull(body.user);
                Guard.ArgumentNotNull(body.password);
                Guard.ArgumentNotNull(body.token);
            }
            catch
            {
                return JResponse.Error(JErrorMessages.InvalidRequest);
            }

            User user = await userRepository.GetByName(body.user);
            if (user == null)
            {
                return JResponse.Error(ErrorUnknownUser);
            }

            if (!userAuthenticator.CheckPassword(user, body.password))
            {
                return JResponse.Error(ErrorAuthenticationFailed);
            }

            string key = userAuthenticator.GetAuthenticationKey(user, body.token);
            return new JGetAuthKeyResponse()
            {
                status = JStatus.OK,
                key = key
            };
        }

        #endregion
    }
}
