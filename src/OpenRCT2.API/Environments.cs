using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OpenRCT2.API
{
    public static class Environments
    {
        public static string Development = "development";
        public static string Testing = "testing";
    }

    internal static class HostingEnvironmentExtensions
    {
        public static bool IsTesting(this IWebHostEnvironment env)
        {
            return env.IsEnvironment(Environments.Testing);
        }
    }
}
