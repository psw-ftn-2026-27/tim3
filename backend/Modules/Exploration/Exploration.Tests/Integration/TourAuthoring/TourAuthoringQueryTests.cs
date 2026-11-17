using System.Net.Http.Json;
using Exploration.Application.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringQueryTests : BaseIntegrationTest
{
    public TourAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);

        tours.Should().HaveCount(TourSeed.All.Count(tour => tour.AuthorId == WellKnownUsers.Explorer));
        tours.Should().OnlyContain(tour => tour.AuthorId == WellKnownUsers.Explorer);
        tours.Should().NotContain(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
    }

    [Fact]
    public async Task Author_sees_the_recommended_group_size_of_each_of_their_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);

        tours.Should().BeEquivalentTo(TourSeed.All
            .Where(tour => tour.AuthorId == WellKnownUsers.Explorer)
            .Select(tour => new
            {
                tour.Id,
                RecommendedGroupSize = new GroupSizeDto(tour.RecommendedGroupSize!.From, tour.RecommendedGroupSize.To)
            }));
    }
}
