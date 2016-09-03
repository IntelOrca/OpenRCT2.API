using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenRCT2.API.Authentication
{
    public class ApiAuthenticationMiddleware : AuthenticationMiddleware<ApiAuthenticationOptions>
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public ApiAuthenticationMiddleware(RequestDelegate next,
                                           IOptions<ApiAuthenticationOptions> options,
                                           ILoggerFactory loggerFactory,
                                           UrlEncoder encoder,
                                           IUserSessionRepository userSessionRepository)
            : base(next, options, loggerFactory, encoder)
        {
            _userSessionRepository = userSessionRepository;
        }

        protected override AuthenticationHandler<ApiAuthenticationOptions> CreateHandler()
        {
            return new ApiAuthenticationHandler(_userSessionRepository);
        }
    }

    public static class ApiAuthenticationApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApiAuthenticationMiddleware>();
        }
    }
}
