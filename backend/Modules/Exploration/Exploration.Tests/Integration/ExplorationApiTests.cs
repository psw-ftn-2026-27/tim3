using Exploration.Contracts;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration;

// Other modules reach Exploration only through IExplorationApi, so the contract is tested the way
// they call it: resolved from the host's container, one call per test.
public class ExplorationApiTests : BaseIntegrationTest
{
    public ExplorationApiTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Another_module_learns_who_the_author_of_a_tour_is()
    {
        var tour = TourSeed.SecondExplorersTrail;
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var authorId = await explorationApi.GetTourAuthorIdAsync(tour.Id);

        authorId.Should().Be(WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Another_module_gets_no_author_for_a_tour_that_does_not_exist()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var authorId = await explorationApi.GetTourAuthorIdAsync(Guid.NewGuid());

        authorId.Should().BeNull();
    }

    [Theory]
    [InlineData(TourStatus.Published, true)]
    [InlineData(TourStatus.Draft, false)]
    [InlineData(TourStatus.Archived, false)]
    public async Task Another_module_can_offer_only_published_tours(TourStatus status, bool expectedAvailability)
    {
        var tour = TourSeed.All.First(tour => tour.Status == status);
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var available = await explorationApi.GetTourAvailabilityAsync(tour.Id);

        available.Should().Be(expectedAvailability);
    }

    [Fact]
    public async Task A_missing_tour_is_unavailable_to_another_module()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();
        var tourId = Guid.NewGuid();

        var available = await explorationApi.GetTourAvailabilityAsync(tourId);

        available.Should().BeFalse();
    }
}
