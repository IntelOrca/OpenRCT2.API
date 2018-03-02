using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Services;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private const string AuthenticationScheme = "Automatic";

        private const string AuthorizationHeaderPrefix = "Bearer";
        private readonly static char[] AuthorizationHeaderSeperator = new char[] { ' ' };

        private readonly UserAuthenticationService _userAuthenticationService;

        public ApiAuthenticationHandler(
            IOptionsMonitor<ApiAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            UserAuthenticationService userAuthenticationService) 
            : base(options, logger, encoder, clock)
        {
            _userAuthenticationService = userAuthenticationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token = GetAuthenticationToken();
            if (token != null)
            {
                var user = await _userAuthenticationService.AuthenticateWithTokenAsync(token);
                if (user != null)
                {
                    var ticket = GetTicketForUser(user, token);
                    return AuthenticateResult.Success(ticket);
                }
                else
                {
                    return AuthenticateResult.Fail(JErrorMessages.InvalidToken);
                }
            }
            else
            {
                var ticket = GetAnonymousTicket();
                return AuthenticateResult.Success(ticket);
            }
        }

        private AuthenticationTicket GetAnonymousTicket()
        {
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, UserRole.Anonymous));
            return GetTicket(claimsIdentity);
        }

        private AuthenticationTicket GetTicketForUser(User user, string token)
        {
            var claimsIdentity = new AuthenticatedUser(user, token);
            return GetTicket(claimsIdentity);
        }

        private AuthenticationTicket GetTicket(IIdentity identity)
        {
            var claimsPrincipal = new ClaimsPrincipal(identity);
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
