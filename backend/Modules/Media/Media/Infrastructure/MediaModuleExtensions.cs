using Media.Core;
using Media.Core.ImageUploading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Media.Infrastructure;

public static class MediaModuleExtensions
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IImageInspector, SkiaImageInspector>();
        services.AddSingleton<IImageStorage, DiskImageStorage>();

        services.AddScoped<ImageUploadingService>();
        services.AddScoped<ImageUploadingQueries>();

        return services;
    }
}
