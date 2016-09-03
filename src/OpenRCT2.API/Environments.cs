using Microsoft.AspNetCore.Hosting;

namespace OpenRCT2.API
{
    public static class Environments
    {
        public static string Testing = "testing";
    }

    internal static class HostingEnvironmentExtensions
    {
        public static bool IsTesting(this IHostingEnvironment env)
        {
            return env.IsEnvironment(Environments.Testing);
        }
    }
}
