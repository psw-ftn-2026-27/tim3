using Maps.Core;
using Maps.Core.WalkingRoutes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maps.Infrastructure;

public static class MapsModuleExtensions
{
    public static IServiceCollection AddMapsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRoutingService, OsrmRoutingService>();

        services.AddScoped<WalkingRoutesQueries>();

        return services;
    }
}
