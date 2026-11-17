using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task Tourist_sees_only_the_name_description_and_price_in_the_tour_preview()
    {
        var tour = TourSeed.PublishedVineyards;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        using var document = await client.GetFromJsonAsync<JsonDocument>($"/api/exploration/tours/{tour.Id}");

        var preview = document!.RootElement;
        preview.EnumerateObject().Select(property => property.Name).Should()
            .BeEquivalentTo(new[] { "name", "description", "priceInEc" });
        preview.GetProperty("name").GetString().Should().Be(tour.Name);
        preview.GetProperty("description").GetString().Should().Be(tour.Description);
        preview.GetProperty("priceInEc").GetInt32().Should().Be(ProductSeed.PublishedVineyards.PriceInEc);
    }

    [Fact]
    public async Task Tourist_cannot_open_an_archived_tour_preview()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.ArchivedHomesteads.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tourist_sees_their_archived_free_tour_and_no_other_buyers_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>(PurchasedToursUrl, JsonOptions);

        tours!.Items.Should().ContainSingle().Which.Id.Should().Be(TourSeed.ArchivedHomesteads.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
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
        var tour = archived ? TourSeed.ArchivedHomesteads : TourSeed.PublishedVineyards;
        var buyer = archived ? WellKnownUsers.SecondExplorer : WellKnownUsers.ThirdExplorer;
        using var client = Factory.CreateClientFor(buyer, "explorer");

        var response = await client.GetAsync($"{PurchasedToursUrl}/{tour.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
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

        var response = await client.GetAsync($"{PurchasedToursUrl}/{TourSeed.PublishedVineyards.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.TryGetProperty("description", out _).Should().BeFalse();
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
        body.RootElement.TryGetProperty("description", out _).Should().BeFalse();
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
