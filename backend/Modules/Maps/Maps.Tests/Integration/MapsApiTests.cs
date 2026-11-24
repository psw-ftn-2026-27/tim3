using FluentAssertions;
using Maps.Contracts;
using Maps.Tests.Integration.Seeds;
using Microsoft.Extensions.DependencyInjection;
using Shared.Tests;
using Xunit;

namespace Maps.Tests.Integration;

// Other modules reach Maps only through IMapsApi, so the contract is tested the way they call it:
// resolved from the host's container, one call per test.
public class MapsApiTests : BaseIntegrationTest
{
    public MapsApiTests(MapsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Another_module_gets_the_coordinates_and_the_time_of_a_users_stored_position()
    {
        var existing = UserPositionSeed.ExplorerAtFreedomSquare;
        using var scope = Factory.Services.CreateScope();
        var mapsApi = scope.ServiceProvider.GetRequiredService<IMapsApi>();

        var position = await mapsApi.GetCurrentPositionAsync(existing.UserId);

        position.Should().Be(new CurrentPosition(
            existing.Coordinates.Latitude, existing.Coordinates.Longitude, existing.SetAt));
    }

    [Fact]
    public async Task Another_module_gets_no_position_for_a_user_who_has_not_set_one_instead_of_a_default_location()
    {
        using var scope = Factory.Services.CreateScope();
        var mapsApi = scope.ServiceProvider.GetRequiredService<IMapsApi>();

        var position = await mapsApi.GetCurrentPositionAsync(WellKnownUsers.SecondExplorer);

        position.Should().BeNull();
    }
}
