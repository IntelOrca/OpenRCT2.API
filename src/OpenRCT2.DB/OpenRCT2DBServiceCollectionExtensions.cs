using Microsoft.Extensions.DependencyInjection;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Repositories;

namespace OpenRCT2.DB
{
    public static class OpenRCT2DBServiceCollectionExtensions
    {
        public static void AddOpenRCT2DB(this IServiceCollection services)
        {
            services.AddSingleton<IDBService, DBService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
            services.AddScoped<INewsItemRepository, NewsItemRepository>();
        }
    }
}
