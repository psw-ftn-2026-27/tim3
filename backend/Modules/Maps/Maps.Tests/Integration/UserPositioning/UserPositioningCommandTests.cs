using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maps.Core.UserPositioning;
using Maps.Infrastructure.Persistence;
using Maps.Tests.Integration.Seeds;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Xunit;

namespace Maps.Tests.Integration.UserPositioning;

public class UserPositioningCommandTests : BaseIntegrationTest
{
    public UserPositioningCommandTests(MapsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_without_a_position_sets_one_and_it_is_stored_with_their_coordinates_and_the_time_it_was_set()
    {
        var userId = WellKnownUsers.SecondExplorer;
        var client = Factory.CreateClientFor(userId, "explorer");
        var request = new SetUserPositionDto(45.2671, 19.8335);
        var requestedAt = DateTime.UtcNow;

        var response = await client.PutAsJsonAsync("/api/maps/user-positions/me", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<MapsDbContext>();
        var position = await context.UserPositions.SingleAsync(position => position.UserId == userId);
        position.Coordinates.Latitude.Should().Be(request.Latitude);
        position.Coordinates.Longitude.Should().Be(request.Longitude);
        position.SetAt.Should().BeOnOrAfter(requestedAt).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task User_who_already_has_a_position_sets_a_new_one_and_still_has_exactly_one_with_the_new_coordinates_and_time()
    {
        var existing = UserPositionSeed.ExplorerAtFreedomSquare;
        var client = Factory.CreateClientFor(existing.UserId, "explorer");
        var request = new SetUserPositionDto(45.2517, 19.8659);

        var response = await client.PutAsJsonAsync("/api/maps/user-positions/me", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<MapsDbContext>();
        var position = await context.UserPositions.SingleAsync(position => position.UserId == existing.UserId);
        position.Id.Should().Be(existing.Id);
        position.Coordinates.Latitude.Should().Be(request.Latitude);
        position.Coordinates.Longitude.Should().Be(request.Longitude);
        position.SetAt.Should().BeAfter(existing.SetAt);
    }

    [Fact]
    public async Task Position_outside_the_allowed_coordinate_range_is_not_stored()
    {
        var existing = UserPositionSeed.ExplorerAtFreedomSquare;
        var client = Factory.CreateClientFor(existing.UserId, "explorer");

        var response = await client.PutAsJsonAsync("/api/maps/user-positions/me", new SetUserPositionDto(91, 19.8335));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<MapsDbContext>();
        var position = await context.UserPositions.SingleAsync(position => position.UserId == existing.UserId);
        position.Coordinates.Should().Be(existing.Coordinates);
        position.SetAt.Should().Be(existing.SetAt);
    }

    [Fact]
    public async Task Anonymous_user_cannot_set_a_position()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/maps/user-positions/me", new SetUserPositionDto(45.2671, 19.8335));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
