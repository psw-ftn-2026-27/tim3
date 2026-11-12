using Microsoft.Extensions.DependencyInjection;

namespace Maps.Api;

public static class MapsApiExtensions
{
    public static IMvcBuilder AddMapsControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(MapsApiExtensions).Assembly);
    }
}
