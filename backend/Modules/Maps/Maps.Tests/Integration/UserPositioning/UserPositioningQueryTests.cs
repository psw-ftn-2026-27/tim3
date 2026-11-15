using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maps.Core.UserPositioning;
using Maps.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Maps.Tests.Integration.UserPositioning;

public class UserPositioningQueryTests : BaseIntegrationTest
{
    public UserPositioningQueryTests(MapsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_with_a_position_gets_its_coordinates_and_the_time_it_was_set()
    {
        var existing = UserPositionSeed.ExplorerAtFreedomSquare;
        var client = Factory.CreateClientFor(existing.UserId, "explorer");

        var response = await client.GetAsync("/api/maps/user-positions/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var position = await response.Content.ReadFromJsonAsync<UserPositionDto>(JsonOptions);
        position.Should().Be(new UserPositionDto(
            existing.Coordinates.Latitude, existing.Coordinates.Longitude, existing.SetAt));
    }

    [Fact]
    public async Task User_without_a_position_is_told_that_it_does_not_exist_instead_of_getting_a_default_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/maps/user-positions/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
