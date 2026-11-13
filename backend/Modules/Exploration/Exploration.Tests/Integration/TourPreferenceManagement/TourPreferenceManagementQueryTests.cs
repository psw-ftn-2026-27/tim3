using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceManagement;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceManagement;

public class TourPreferenceManagementQueryTests : BaseIntegrationTest
{
    private const string Address = "/api/exploration/tour-preferences/mine";

    public TourPreferenceManagementQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_their_own_tour_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expected = TourPreferenceSeed.ExplorersEasyHistoryWalks;

        var preference = await client.GetFromJsonAsync<TourPreferenceDto>(Address, JsonOptions);

        preference!.Id.Should().Be(expected.Id);
        preference.Difficulties.Should().Equal(expected.Difficulties);
        preference.Transport.Should().Be(expected.Transport);
        preference.Tags.Should().Equal(expected.Tags);
        preference.AppliesToSearch.Should().Be(expected.AppliesToSearch);
    }

    [Fact]
    public async Task Explorer_without_a_tour_preference_sees_none()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.GetAsync(Address);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
