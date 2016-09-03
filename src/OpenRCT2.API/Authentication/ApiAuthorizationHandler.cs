using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Implementations;

namespace OpenRCT2.API.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private const string AuthenticationScheme = "Automatic";
        private const string AuthenticationType = "token";

        private const string AuthorizationHeaderPrefix = "Bearer";
        private readonly static char[] AuthorizationHeaderSeperator = new char[] { ' ' };

        private readonly IUserSessionRepository _userSessionRepository;

        public ApiAuthenticationHandler(IUserSessionRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token = GetAuthenticationToken();
            if (token != null)
            {
                int? userId = await _userSessionRepository.GetUserIdFromToken(token);
                if (userId.HasValue)
                {
                    AuthenticationTicket ticket = GetTicketForUserId(userId.Value);
                    return AuthenticateResult.Success(ticket);
                }
            }
            return AuthenticateResult.Fail(JErrorMessages.InvalidToken);
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            HttpResponse httpResponse = Context.Response;
            httpResponse.Headers[HeaderNames.ContentType] = MimeTypes.ApplicationJson;

            string response = JsonConvert.SerializeObject(JResponse.Error(JErrorMessages.InvalidToken));
            await httpResponse.WriteAsync(response);
            return false;
        }

        private AuthenticationTicket GetTicketForUserId(int userId)
        {
            var claimsIdentity = new ClaimsIdentity(AuthenticationType);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            // claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, "user@example.com"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authenticationProperties = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(claimsPrincipal, authenticationProperties, AuthenticationScheme);
            return ticket;
        }

        private string GetAuthenticationToken()
        {
            string authorization = Context.Request.Headers[HeaderNames.Authorization];
            if (authorization != null)
            {
                string[] authorizationParts = authorization.Split(AuthorizationHeaderSeperator, StringSplitOptions.RemoveEmptyEntries);
                if (authorizationParts.Length >= 2 &&
                    authorizationParts[0] == AuthorizationHeaderPrefix)
                {
                    string token = authorizationParts[1];
                    return token;
                }
            }
            return null;
        }
    }
}
