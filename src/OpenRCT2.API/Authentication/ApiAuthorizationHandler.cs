using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace OpenRCT2.API.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        public ApiAuthenticationHandler(IOptionsMonitor<ApiAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Get Authorization header value
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorization))
            {
                return Task.FromResult(AuthenticateResult.Fail("Cannot read authorization header."));
            }

            // The auth key from Authorization header check against the configured ones
            if (authorization.Any(key => Options.AuthKey.All(ak => ak != key)))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid auth key."));
            }

            // Create authenticated user
            var identities = new List<ClaimsIdentity> {new ClaimsIdentity("custom auth type")};
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identities), Options.Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public static class AuthenticationBuilderExtensions
    {
        // Custom authentication extension method
        public static AuthenticationBuilder AddApiAuthentication(this AuthenticationBuilder builder, Action<ApiAuthenticationOptions> configureOptions)
        {
            // Add custom authentication scheme with custom options and custom handler
            return builder.AddScheme<ApiAuthenticationOptions, ApiAuthenticationHandler>(ApiAuthenticationOptions.DefaultScheme, configureOptions);
        }
    }
}
