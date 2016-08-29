using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.AppVeyor;
using OpenRCT2.API.Implementations;

namespace OpenRCT2.API
{
    public class Startup
    {
        private const int DefaultPort = 5004;

        private readonly string[] AllowedOrigins = new string[]
        {
            "https://openrct2.website",
            "https://ui.openrct2.website",
            "http://localhost:3000",
        };

        public Startup(IHostingEnvironment env)
        {
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Random>();
            services.AddSingleton<IUserAuthenticator, UserAuthenticator>();
            services.AddSingleton<IServerRepository, ServerRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IAppVeyorService, AppVeyorService>();
            services.AddSingleton<ILocalisationService, LocalisationService>();
            services.AddMvc();
            services.AddCors();
        }

        public void Configure(IServiceProvider serviceProvider,
                              IApplicationBuilder app,
                              IHostingEnvironment env,
                              ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug();

#if DEBUG
            app.UseDeveloperExceptionPage();
#endif

            // Allow certain domains for AJAX / JSON capability
            app.UseCors(builder => builder.WithOrigins(AllowedOrigins));

#if _ENABLE_CHAT_
            app.Map("/chat", wsapp => {
                wsapp.UseWebSockets(new WebSocketOptions {
                    ReplaceFeature = true
                });

                wsapp.Use(async (context, next) =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        using (var wsSession = new WebSocketSession(serviceProvider, webSocket))
                        {
                            await wsSession.Run();
                        }
                        return;
                    }
                    await next();
                });
            });
#endif

            // Redirect servers.openrct2.website to /servers
            // servers.openrct2.website
            app.Use(async (context, next) =>
            {
                string host = context.Request.Host.Value;
                if (String.Equals(host, "servers.openrct2.website", StringComparison.OrdinalIgnoreCase))
                {
                    context.Request.Path = "/servers";
                }
                await next();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();

            // Let index redirect to main website
            app.MapWhen(context => context.Request.Path == "/", app2 =>
            {
                app2.Use((context, next) =>
                {
                    context.Response.Redirect("https://openrct2.website");
                    return Task.FromResult(0);
                });
            });

            // Fallback to an empty 404
            app.Run(context =>
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.FromResult(0);
            });
        }

        // Entry point for the application.
        public static int Main(string[] args)
        {
            PrintArguments(args);

            string bindAddress = $"http://localhost:{DefaultPort}";
            if (args.Length >= 2)
            {
                bindAddress = args[1];
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(bindAddress)
                .Build();

            host.Run();
            return 0;
        }

        private static void PrintArguments(string[] args)
        {
            Console.WriteLine("Starting OpenRCT2.API with arguments:");
            foreach (string arg in args)
            {
                Console.WriteLine("  " + arg);
            }
            Console.WriteLine("---------------");
        }
    }
}
