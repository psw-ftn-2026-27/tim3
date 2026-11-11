using System.Net;
using FluentAssertions;
using Maps.Core;
using Maps.Core.WalkingRoutes;
using Maps.Infrastructure;
using Xunit;

namespace Maps.Tests.Unit;

public class OsrmRoutingServiceTests
{
    private const string FoundRoute = """
        {
          "code": "Ok",
          "routes": [
            {
              "distance": 2490,
              "geometry": { "coordinates": [[19.8451, 45.2551], [19.8490, 45.2560], [19.8521, 45.2556]] }
            }
          ]
        }
        """;

    [Fact]
    public async Task Found_route_becomes_points_along_the_streets_and_its_length_in_kilometres()
    {
        var routing = Routing(HttpStatusCode.OK, FoundRoute);

        var route = await routing.FindWalkingRouteAsync(TwoLocations());

        route.Should().BeEquivalentTo(new WalkingRouteDto(
            [new LocationDto(45.2551, 19.8451), new LocationDto(45.2560, 19.8490), new LocationDto(45.2556, 19.8521)],
            2.49));
    }

    [Theory]
    [InlineData("NoRoute")]
    [InlineData("NoSegment")]
    public async Task Route_is_not_found_when_the_service_reports_no_walking_path(string code)
    {
        var routing = Routing(HttpStatusCode.BadRequest, $$"""{ "code": "{{code}}" }""");

        var route = await routing.FindWalkingRouteAsync(TwoLocations());

        route.Should().BeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, """{ "code": "InvalidQuery" }""")]
    [InlineData(HttpStatusCode.TooManyRequests, "")]
    [InlineData(HttpStatusCode.InternalServerError, "")]
    public async Task Any_other_service_failure_remains_an_error(HttpStatusCode status, string body)
    {
        var routing = Routing(status, body);

        var finding = () => routing.FindWalkingRouteAsync(TwoLocations());

        await finding.Should().ThrowAsync<HttpRequestException>();
    }

    private static OsrmRoutingService Routing(HttpStatusCode status, string body)
    {
        return new OsrmRoutingService(new FakeOsrmHandler(status, body));
    }

    private static RouteLocations TwoLocations()
    {
        return new RouteLocations([new LocationDto(45.2551, 19.8451), new LocationDto(45.2556, 19.8521)]);
    }
}
