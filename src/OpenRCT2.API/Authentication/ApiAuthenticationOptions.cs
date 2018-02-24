using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;

namespace OpenRCT2.API.Authentication
{
    public class ApiAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "openrct2.io";
        public string Scheme => DefaultScheme;
        public StringValues AuthKey { get; set; }
    }
}
