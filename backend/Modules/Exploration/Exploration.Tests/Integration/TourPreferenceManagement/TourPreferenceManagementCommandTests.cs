using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceManagement;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceManagement;

public class TourPreferenceManagementCommandTests : BaseIntegrationTest
{
    private const string Address = "/api/exploration/tour-preferences/mine";

    public TourPreferenceManagementCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_saves_a_first_tour_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var request = new SaveTourPreferenceDto([TourDifficulty.Hard], TransportMode.Bicycle, ["priroda"], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync(Address, request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore + 1);
        var stored = assertContext.TourPreferences.Single(preference => preference.ExplorerId == WellKnownUsers.ThirdExplorer);
        stored.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Transport.Should().Be(TransportMode.Bicycle);
        stored.Tags.Should().Equal("priroda");
        stored.AppliesToSearch.Should().BeTrue();
    }

    [Fact]
    public async Task Explorer_changes_their_tour_preference_instead_of_adding_another()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new SaveTourPreferenceDto([], null, ["vino", "grad"], false);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync(Address, request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        var stored = assertContext.TourPreferences.Single(preference => preference.Id == TourPreferenceSeed.ExplorersEasyHistoryWalks.Id);
        stored.Difficulties.Should().BeEmpty();
        stored.Transport.Should().BeNull();
        stored.Tags.Should().Equal("vino", "grad");
        stored.AppliesToSearch.Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_cannot_save_an_empty_tour_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var request = new SaveTourPreferenceDto([], null, [], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync(Address, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_save_a_tour_preference()
    {
        var client = Factory.CreateClient();
        var request = new SaveTourPreferenceDto([TourDifficulty.Easy], null, [], false);

        var response = await client.PutAsJsonAsync(Address, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_save_a_tour_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new SaveTourPreferenceDto([TourDifficulty.Easy], null, [], false);

        var response = await client.PutAsJsonAsync(Address, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Explorer_deletes_their_tour_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(Address);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Should().NotContain(preference => preference.Id == TourPreferenceSeed.ExplorersEasyHistoryWalks.Id);
        assertContext.TourPreferences.Should().Contain(preference => preference.Id == TourPreferenceSeed.SecondExplorersCarTrips.Id);
    }

    [Fact]
    public async Task Explorer_without_a_tour_preference_has_nothing_to_delete()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync(Address);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
    }
}
