using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Diagnostics;
using OpenRCT2.API.Extensions;
using OpenRCT2.API.Implementations;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Controllers
{
    public class UserController
    {
        public const string ErrorUnknownUser = "unknown user";
        public const string ErrorAuthenticationFailed = "authentication failed";

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

        #endregion

        [HttpPost("user/login")]
        public async Task<IJResponse> Login(
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

            OpenRCT2org.JUser orgUser;
            try
            {
                var orgAPI = new OpenRCT2org.UserAPI();
                orgUser = await orgAPI.AuthenticateUser(body.user, body.password);
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

        #region Old

        [HttpPost("user/getauthsession")]
        public async Task<IJResponse> GetAuthenticationSession(
            [FromServices] Random random,
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
                var orgAPI = new OpenRCT2org.UserAPI();
                orgUser = await orgAPI.AuthenticateUser(body.user, body.password);
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
