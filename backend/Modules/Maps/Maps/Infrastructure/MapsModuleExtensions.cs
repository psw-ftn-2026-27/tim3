using Maps.Contracts;
using Maps.Core;
using Maps.Core.UserPositioning;
using Maps.Core.UserPositions;
using Maps.Core.WalkingRoutes;
using Maps.Infrastructure.Persistence;
using Maps.Infrastructure.Persistence.UserPositions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Maps.Infrastructure;

public static class MapsModuleExtensions
{
    public static IServiceCollection AddMapsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<MapsDbContext>(configuration, "maps");
        services.AddHostedService<MapsModuleInitializer>();

        services.AddSingleton<IRoutingService, OsrmRoutingService>();
        services.AddScoped<WalkingRoutesQueries>();

        services.AddScoped<IUserPositionRepository, UserPositionRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<MapsDbContext>());
        services.AddScoped<IUserPositionReadRepository, UserPositionReadRepository>();
        services.AddScoped<UserPositioningService>();
        services.AddScoped<UserPositioningQueries>();
        services.AddScoped<IMapsApi, MapsApi>();

        return services;
    }
}
