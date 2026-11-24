using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maps.Core.WalkingRoutes;
using Shared.Tests;
using Xunit;

namespace Maps.Tests.Integration.WalkingRoutes;

public class WalkingRoutesQueryTests : BaseIntegrationTest
{
    public WalkingRoutesQueryTests(MapsApiFactory factory) : base(factory) { }

    private static readonly LocationDto Start = new(45.2671, 19.8335);
    private static readonly LocationDto Finish = new(45.2551, 19.8451);

    private static string AddressFor(params LocationDto[] locations)
    {
        var query = locations.Select((location, index) => string.Create(CultureInfo.InvariantCulture,
            $"locations[{index}].latitude={location.Latitude}&locations[{index}].longitude={location.Longitude}"));
        return $"/api/maps/walking-routes?{string.Join("&", query)}";
    }

    [Fact]
    public async Task Walking_route_through_the_given_locations_has_the_points_and_length_in_kilometres_the_routing_service_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(AddressFor(Start, Finish));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<WalkingRouteDto>(JsonOptions);
        body!.Points.Should().Equal(FakeRoutingService.Route.Points);
        body.LengthInKilometres.Should().Be(FakeRoutingService.Route.LengthInKilometres);
    }

    [Fact]
    public async Task Walking_route_with_a_part_the_routing_service_cannot_find_ends_in_an_error_instead_of_an_empty_or_partial_route()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(AddressFor(Start, FakeRoutingService.Unreachable, Finish));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_get_a_walking_route()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(AddressFor(Start, Finish));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
