using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebSockets.Server;
using Microsoft.AspNetCore.Cors;
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

            app.UseCors(builder => 
                builder.WithOrigins("https://openrct2.website", "https://ui.openrct2.website", "http://localhost:3000"));

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

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }

        // Entry point for the application.
        public static int Main(string[] args)
        {
            PrintArguments(args);

            string bindAddress = $"http://localhost:{DefaultPort}";
            if (args.Length >= 1)
            {
                bindAddress = args[0];
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
