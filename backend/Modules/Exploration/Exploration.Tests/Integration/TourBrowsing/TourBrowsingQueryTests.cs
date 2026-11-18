using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Archived_tour_is_not_listed_among_published_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().NotContain(tour => tour.Id == TourSeed.ArchivedHomesteads.Id);
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Published_tours_reveal_no_key_point_secret()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var secrets = TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published)
            .SelectMany(tour => tour.KeyPoints)
            .Select(keyPoint => keyPoint.Secret)
            .OfType<string>();

        var body = await client.GetStringAsync("/api/exploration/tours/published?pageSize=100");

        var text = Regex.Unescape(body);
        text.Should().Contain(TourSeed.PublishedVineyards.Id.ToString());
        foreach (var secret in secrets)
        {
            text.Should().NotContain(secret);
        }
    }

    [Fact]
    public async Task Tourist_sees_the_details_and_only_the_visible_key_points_of_a_published_tour_in_order()
    {
        var tour = TourSeed.PublishedOldTownWithHiddenMiddleKeyPoint;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var preview = await client.GetFromJsonAsync<TourPreviewDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        preview.Should().BeEquivalentTo(new
        {
            tour.Name,
            tour.Description,
            tour.Difficulty,
            tour.Tags,
            RecommendedGroupSize = new GroupSizeDto(tour.RecommendedGroupSize!.From, tour.RecommendedGroupSize.To),
            TransportTimes = tour.TransportTimes.Select(time => new TransportTimeDto(time.Transport, time.Minutes)),
            Length = new TourLengthDto(tour.Length.Kilometres, tour.Length.IsStraightLine),
            tour.Status,
            IsOwnTour = false,
            KeyPoints = tour.KeyPoints
                .Where(keyPoint => keyPoint.VisibleWithoutPurchase)
                .OrderBy(keyPoint => keyPoint.Order)
                .Select(keyPoint => new PreviewKeyPointDto(
                    keyPoint.Name,
                    keyPoint.Description,
                    keyPoint.ImageAddress,
                    new GeoLocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Tour_preview_reveals_no_key_point_secret()
    {
        var tour = TourSeed.PublishedOldTownWithHiddenMiddleKeyPoint;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var secrets = tour.KeyPoints
            .Select(keyPoint => keyPoint.Secret)
            .OfType<string>();

        var body = await client.GetStringAsync($"/api/exploration/tours/{tour.Id}");

        var text = Regex.Unescape(body);
        text.Should().Contain(tour.Name);
        foreach (var secret in secrets)
        {
            text.Should().NotContain(secret);
        }
    }

    [Fact]
    public async Task Tourist_cannot_preview_another_authors_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tourist_cannot_preview_another_authors_archived_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.ArchivedHomesteads.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tourist_cannot_preview_a_tour_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Author_previews_their_own_draft_tour_marked_as_their_own()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var preview = await client.GetFromJsonAsync<TourPreviewDto>($"/api/exploration/tours/{TourSeed.FortressWalk.Id}", JsonOptions);

        preview!.IsOwnTour.Should().BeTrue();
    }

    [Fact]
    public async Task Author_previews_their_own_archived_tour_marked_as_their_own()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var preview = await client.GetFromJsonAsync<TourPreviewDto>($"/api/exploration/tours/{TourSeed.ArchivedHomesteads.Id}", JsonOptions);

        preview!.IsOwnTour.Should().BeTrue();
    }

    [Fact]
    public async Task Anonymous_user_cannot_preview_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.PublishedOldTownWithHiddenMiddleKeyPoint.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
