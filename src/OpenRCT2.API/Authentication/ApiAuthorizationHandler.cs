using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OpenRCT2.API.Abstractions;

namespace OpenRCT2.API.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private const string AuthenticationScheme = "Automatic";
        private const string AuthenticationType = "token";

        private const string AuthorizationHeaderPrefix = "Bearer";
        private readonly static char[] AuthorizationHeaderSeperator = new char[] { ' ' };

        private readonly IUserSessionRepository _userSessionRepository;

        public ApiAuthenticationHandler(
            IOptionsMonitor<ApiAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserSessionRepository userSessionRepository) 
            : base(options, logger, encoder, clock)
        {
            _userSessionRepository = userSessionRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token = GetAuthenticationToken();
            if (token != null)
            {
                int? userId = await _userSessionRepository.GetUserIdFromTokenAsync(token);
                if (userId.HasValue)
                {
                    var ticket = GetTicketForUserId(userId.Value);
                    return AuthenticateResult.Success(ticket);
                }
            }
            return AuthenticateResult.Fail(JErrorMessages.InvalidToken);
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

    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiAuthentication(
            this AuthenticationBuilder builder,
            Action<ApiAuthenticationOptions> configureOptions = null)
        {
            return builder.AddScheme<ApiAuthenticationOptions, ApiAuthenticationHandler>(
                ApiAuthenticationOptions.DefaultScheme,
                configureOptions);
        }
    }
}
