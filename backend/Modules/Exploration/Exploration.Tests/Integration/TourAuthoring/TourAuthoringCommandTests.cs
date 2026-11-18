using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringCommandTests : BaseIntegrationTest
{
    public TourAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    private static DefineKeyPointsDto CreateKeyPointsDefinition() => new(
    [
        new KeyPointInputDto("Štrand", "Gradska plaža na Dunavu.", "/api/media/images/strand.png", new GeoLocationDto(45.2420, 19.8570), "Ispod spasilačke kule je stari kamen sa urezanom godinom.", true),
        new KeyPointInputDto("Kej žrtava racije", "Spomenik na keju.", "/api/media/images/kej.png", new GeoLocationDto(45.2490, 19.8560), null, false),
        new KeyPointInputDto("Most slobode", "Kraj staze, ispod Mosta slobode.", "/api/media/images/most-slobode.png", new GeoLocationDto(45.2410, 19.8560), "", false)
    ], new TourLengthDto(3.2, true));

    [Fact]
    public async Task Explorer_creates_a_draft_tour_with_a_recommended_group_size()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"], new GroupSizeDto(2, 6));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore + 1);
        var stored = assertContext.Tours.Single(tour => tour.Id == created.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.TransportTimes.Should().BeEmpty();
        stored.RecommendedGroupSize.Should().Be(new GroupSize(2, 6));
    }

    [Fact]
    public async Task Explorer_cannot_create_a_tour_without_a_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("   ", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new GroupSizeDto(2, 6));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_tour_with_an_invalid_group_size()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new GroupSizeDto(6, 2));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new GroupSizeDto(2, 6));

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new GroupSizeDto(2, 6));

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_adds_a_transport_time_to_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TransportTimeDto(TransportMode.Walking, 120);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.TransportTimes.Should().ContainSingle(time => time.Transport == TransportMode.Walking && time.Minutes == 120);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, 1500)]
    public async Task Author_publishes_a_free_or_paid_tour(bool isFree, int? amount)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PublishTourDto(isFree, amount);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.PublishedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task Author_cannot_publish_a_tour_without_a_chosen_price_or_a_paid_amount(bool? isFree)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PublishTourDto(isFree, null);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var request = new PublishTourDto(true, null);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new PublishTourDto(true, null);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_publish_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new PublishTourDto(true, null);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_archives_their_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Archived);
        stored.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Explorer_cannot_archive_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Published);
    }

    [Fact]
    public async Task Author_cannot_archive_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_archive_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_reactivates_their_archived_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedHomesteads.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedHomesteads.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Explorer_cannot_reactivate_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedHomesteads.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedHomesteads.Id);
        stored.Status.Should().Be(TourStatus.Archived);
    }

    [Fact]
    public async Task Author_cannot_reactivate_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reactivate_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedHomesteads.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reactivate_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedHomesteads.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_saves_the_key_points_of_their_draft_tour_replacing_the_old_ones()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = CreateKeyPointsDefinition();
        var oldKeyPointIds = TourSeed.PublishableRiverside.KeyPoints.Select(keyPoint => keyPoint.Id).ToList();

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.OrderBy(keyPoint => keyPoint.Order).Select(keyPoint => keyPoint.Name)
            .Should().Equal(request.KeyPoints.Select(keyPoint => keyPoint.Name));
        stored.Length.Should().Be(new TourLength(3.2, true));
        assertContext.Set<KeyPoint>().Any(keyPoint => oldKeyPointIds.Contains(keyPoint.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_cannot_save_the_key_points_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", CreateKeyPointsDefinition());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.Select(keyPoint => keyPoint.Id)
            .Should().BeEquivalentTo(TourSeed.PublishableRiverside.KeyPoints.Select(keyPoint => keyPoint.Id));
    }

    [Fact]
    public async Task Author_cannot_save_the_key_points_of_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/key-points", CreateKeyPointsDefinition());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.KeyPoints.Select(keyPoint => keyPoint.Id)
            .Should().BeEquivalentTo(TourSeed.PublishedVineyards.KeyPoints.Select(keyPoint => keyPoint.Id));
    }

    [Fact]
    public async Task Anonymous_user_cannot_save_key_points()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", CreateKeyPointsDefinition());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_save_key_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", CreateKeyPointsDefinition());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
