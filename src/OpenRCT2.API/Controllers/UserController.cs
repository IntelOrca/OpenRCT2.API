using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Diagnostics;
using OpenRCT2.API.Implementations;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.Controllers
{
    public class UserController
    {
        public const string ErrorUnknownUser = "unknown user";
        public const string ErrorAuthenticationFailed = "authentication failed";

        #region Request / Response Models

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

        #endregion

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
    }
}
