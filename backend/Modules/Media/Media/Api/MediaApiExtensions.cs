using Microsoft.Extensions.DependencyInjection;

namespace Media.Api;

public static class MediaApiExtensions
{
    public static IMvcBuilder AddMediaControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(MediaApiExtensions).Assembly);
    }
}
