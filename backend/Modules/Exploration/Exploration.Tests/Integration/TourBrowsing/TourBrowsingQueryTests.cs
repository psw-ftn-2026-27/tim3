using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    private const string PurchasedToursUrl = "/api/exploration/tours/purchased";

    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Archived_tour_is_not_listed_among_published_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().NotContain(tour => tour.Id == TourSeed.ArchivedHomesteads.Id);
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Tourist_sees_the_price_of_a_tour_with_an_offer_and_no_price_for_a_legacy_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedVineyards.Id)
            .Which.PriceInEc.Should().Be(ProductSeed.PublishedVineyards.PriceInEc);
        tours.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedMonasteries.Id)
            .Which.PriceInEc.Should().BeNull();
    }

    [Fact]
    public async Task Tourist_sees_the_offer_price_in_a_tour_preview()
    {
        var tour = TourSeed.PublishedVineyards;
        using var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var preview = await client.GetFromJsonAsync<TourPreviewDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        preview!.PriceInEc.Should().Be(ProductSeed.PublishedVineyards.PriceInEc);
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

    [Fact]
    public async Task Tourist_sees_their_archived_free_tour_and_no_other_buyers_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>(PurchasedToursUrl, JsonOptions);

        tours!.Items.Should().ContainSingle().Which.Id.Should().Be(TourSeed.ArchivedHomesteads.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedOldTownWithHiddenMiddleKeyPoint.Id);
    }

    [Fact]
    public async Task Tourist_without_purchases_sees_an_empty_list()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.FourthExplorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>(PurchasedToursUrl, JsonOptions);

        tours!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Guest_cannot_see_purchased_tours()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(PurchasedToursUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_purchased_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync(PurchasedToursUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Buyer_can_read_their_purchased_tour_including_after_archiving(bool archived)
    {
        var tour = archived ? TourSeed.ArchivedHomesteads : TourSeed.PublishedOldTownWithHiddenMiddleKeyPoint;
        var buyer = archived ? WellKnownUsers.SecondExplorer : WellKnownUsers.ThirdExplorer;
        using var client = Factory.CreateClientFor(buyer, "explorer");

        var response = await client.GetAsync($"{PurchasedToursUrl}/{tour.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var purchased = body.RootElement.Deserialize<PurchasedTourDetailsDto>(JsonOptions)!;
        var details = purchased.Tour;
        purchased.KeyPoints.Should().BeEquivalentTo(tour.KeyPoints.OrderBy(point => point.Order)
            .Select(point => new PreviewKeyPointDto(point.Name, point.Description, point.ImageAddress,
                new GeoLocationDto(point.Location.Latitude, point.Location.Longitude))),
            options => options.WithStrictOrdering());

        foreach (var point in body.RootElement.GetProperty("keyPoints").EnumerateArray())
        {
            point.EnumerateObject().Select(property => property.Name)
                .Should().BeEquivalentTo(new[] { "name", "description", "imageAddress", "location" });
        }
        var payload = Regex.Unescape(body.RootElement.GetRawText());
        foreach (var secret in tour.KeyPoints.Select(point => point.Secret).OfType<string>())
        {
            payload.Should().NotContain(secret);
        }
        details!.Id.Should().Be(tour.Id);
        details.Name.Should().Be(tour.Name);
        details.Description.Should().Be(tour.Description);
        details.Status.Should().Be(tour.Status);
        details.TransportTimes.Select(time => time.Minutes)
            .Should().BeEquivalentTo(tour.TransportTimes.Select(time => time.Minutes));
    }

    [Fact]
    public async Task Another_tourist_cannot_read_a_buyers_tour()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.FourthExplorer, "explorer");

        var response = await client.GetAsync($"{PurchasedToursUrl}/{TourSeed.PublishedOldTownWithHiddenMiddleKeyPoint.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.TryGetProperty("tour", out _).Should().BeFalse();
        body.RootElement.TryGetProperty("keyPoints", out _).Should().BeFalse();
        body.RootElement.TryGetProperty("transportTimes", out _).Should().BeFalse();
    }

    [Fact]
    public async Task A_purchase_for_a_missing_tour_does_not_expose_details()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"{PurchasedToursUrl}/{TourPurchaseTokenSeed.MissingTourForSecondExplorer.TourId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.TryGetProperty("tour", out _).Should().BeFalse();
        body.RootElement.TryGetProperty("keyPoints", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Failed_ownership_lookup_returns_no_tour_data()
    {
        var tour = TourSeed.ArchivedHomesteads;
        var buyer = WellKnownUsers.SecondExplorer;
        using var paymentContext = Factory.CreateContext<DbContext>();
        var connectionString = paymentContext.Database.GetDbConnection().ConnectionString;
        var withTimeout = new NpgsqlConnectionStringBuilder(connectionString) { CommandTimeout = 2 };
        using var host = Factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:Database", withTimeout.ConnectionString));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Factory.CreateAccessToken(buyer, "explorer"));
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        // Hold only the ownership table so the real Payment query times out.
        await using var command = new NpgsqlCommand(
            """LOCK TABLE payment."TourPurchaseTokens" IN ACCESS EXCLUSIVE MODE""", connection, transaction);
        await command.ExecuteNonQueryAsync();

        var response = await client.GetAsync($"{PurchasedToursUrl}/{tour.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.TryGetProperty("tour", out _).Should().BeFalse();
        body.RootElement.TryGetProperty("keyPoints", out _).Should().BeFalse();
        Regex.Unescape(body.RootElement.GetRawText()).Should().NotContain(tour.Name).And.NotContain(tour.Description);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Purchased_details_require_an_authenticated_tourist(string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.GetAsync($"{PurchasedToursUrl}/{TourSeed.ArchivedHomesteads.Id}");

        response.StatusCode.Should().Be(expected);
    }
}
