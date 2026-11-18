using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
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

    [Fact]
    public async Task Author_reads_the_key_points_of_their_tour_in_order_with_secrets_and_visibility()
    {
        var tour = TourSeed.PublishableRiverside;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var keyPoints = await client.GetFromJsonAsync<TourKeyPointsDto>($"/api/exploration/tours/{tour.Id}/key-points", JsonOptions);

        keyPoints!.Id.Should().Be(tour.Id);
        keyPoints.KeyPoints.Should().BeEquivalentTo(
            tour.KeyPoints.Select(keyPoint => new { keyPoint.Id, keyPoint.Order, keyPoint.Secret, keyPoint.VisibleWithoutPurchase }),
            options => options.WithStrictOrdering());
        keyPoints.Length.Should().Be(new TourLengthDto(tour.Length.Kilometres, tour.Length.IsStraightLine));
    }

    [Fact]
    public async Task Explorer_cannot_read_the_key_points_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
