using System.Net;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Maps.Tests.Integration.UserPositioning;

public class UserPositioningQueryTests : BaseIntegrationTest
{
    public UserPositioningQueryTests(MapsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_without_a_position_is_told_that_it_does_not_exist_instead_of_getting_a_default_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/maps/user-positions/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
