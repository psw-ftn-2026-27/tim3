using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exploration.Application.TourBrowsing;
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
}
