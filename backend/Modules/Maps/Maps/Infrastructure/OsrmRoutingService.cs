using System.Globalization;
using System.Net;
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
        : this(new HttpClientHandler())
    {
    }

    // Jedinični testovi predaju lažni handler sa unapred zadatim odgovorom OSRM-a, pa ne idu na mrežu.
    internal OsrmRoutingService(HttpMessageHandler handler)
    {
        _http = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress), Timeout = TimeSpan.FromSeconds(10) };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Explorer/1.0");
    }

    public async Task<WalkingRouteDto?> FindWalkingRouteAsync(RouteLocations route)
    {
        var coordinates = string.Join(";", route.Locations.Select(ToOsrmCoordinate));

        using var response = await _http.GetAsync($"{coordinates}?overview=full&geometries=geojson");
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // OSRM javlja da puta nema statusom 400 i kodom NoRoute (nema puta) ili NoSegment
            // (lokacija je predaleko od ijedne staze). Svaki drugi neuspeh je greška servisa.
            var error = await response.Content.ReadFromJsonAsync<OsrmResponse>();
            if (error?.Code is "NoRoute" or "NoSegment")
            {
                return null;
            }
        }
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OsrmResponse>();
        var found = body?.Routes?.FirstOrDefault();
        if (body?.Code != "Ok" || found is null)
        {
            return null;
        }

        var points = found.Geometry.Coordinates
            .Select(coordinate => new LocationDto(coordinate[1], coordinate[0]))
            .ToList();
        return new WalkingRouteDto(points, found.Distance / MetresPerKilometre);
    }

    private static string ToOsrmCoordinate(LocationDto location) =>
        string.Create(CultureInfo.InvariantCulture, $"{location.Longitude},{location.Latitude}");

    private sealed record OsrmResponse(string Code, List<OsrmRoute>? Routes);

    private sealed record OsrmRoute(double Distance, OsrmGeometry Geometry);

    private sealed record OsrmGeometry(List<double[]> Coordinates);
}
