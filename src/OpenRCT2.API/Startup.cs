using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddMvc();
        }

        public void Configure(IServiceProvider serviceProvider,
                              IApplicationBuilder app,
                              IHostingEnvironment env,
                              ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

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

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }

        // Entry point for the application.
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:6543")
                .Build();

            host.Run();
        }
    }
}
