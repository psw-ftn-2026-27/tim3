using System.Globalization;
using System.Net.Http.Json;
using Maps.Core;
using Maps.Core.WalkingRoutes;

namespace Maps.Infrastructure;

internal sealed class OsrmRoutingService : IRoutingService
{
    private const string BaseAddress = "https://routing.openstreetmap.de/routed-foot/route/v1/foot/";
    private const double MetresPerKilometre = 1000;

    private readonly HttpClient _http;

    public OsrmRoutingService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseAddress), Timeout = TimeSpan.FromSeconds(10) };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Explorer/1.0");
    }

    public async Task<WalkingRouteDto?> FindWalkingRouteAsync(IReadOnlyList<LocationDto> locations)
    {
        var coordinates = string.Join(";", locations.Select(ToOsrmCoordinate));

        using var response = await _http.GetAsync($"{coordinates}?overview=full&geometries=geojson");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadFromJsonAsync<OsrmResponse>();
        var route = body?.Routes?.FirstOrDefault();
        if (body?.Code != "Ok" || route is null)
        {
            return null;
        }

        var points = route.Geometry.Coordinates
            .Select(coordinate => new LocationDto(coordinate[1], coordinate[0]))
            .ToList();
        return new WalkingRouteDto(points, route.Distance / MetresPerKilometre);
    }

    private static string ToOsrmCoordinate(LocationDto location) =>
        string.Create(CultureInfo.InvariantCulture, $"{location.Longitude},{location.Latitude}");

    private sealed record OsrmResponse(string Code, List<OsrmRoute>? Routes);

    private sealed record OsrmRoute(double Distance, OsrmGeometry Geometry);

    private sealed record OsrmGeometry(List<double[]> Coordinates);
}
