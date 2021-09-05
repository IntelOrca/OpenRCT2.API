using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AuthorisationService>();
            builder.Services.AddScoped<OpenRCT2ApiService>();

            var host = builder.Build();

            // Initialise
            var authService = host.Services.GetService<AuthorisationService>();
            await authService.SetFromLocalStorageAsync();

            // Run
            await host.RunAsync();
        }
    }
}
