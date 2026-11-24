using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubManagement;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubManagement;

public class ClubManagementCommandTests : BaseIntegrationTest
{
    public ClubManagementCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_founds_a_club_and_becomes_its_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateClubDto("Biciklisti", "Klub ljubitelja biciklističkih tura.", ["/api/media/images/bicikl.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore + 1);
        var stored = assertContext.Clubs.Single(club => club.Id == createdId);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Name.Should().Be("Biciklisti");
        stored.Images.Should().ContainSingle().Which.Should().Be("/api/media/images/bicikl.jpg");
    }

    [Fact]
    public async Task Club_named_like_an_existing_club_in_different_case_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new CreateClubDto(ClubSeed.Hikers.Name.ToUpperInvariant(), "Još jedan planinarski klub.", ["/api/media/images/planina.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_found_a_club()
    {
        var client = Factory.CreateClient();
        var request = new CreateClubDto("Biciklisti", "Klub ljubitelja biciklističkih tura.", ["/api/media/images/bicikl.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_found_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateClubDto("Biciklisti", "Klub ljubitelja biciklističkih tura.", ["/api/media/images/bicikl.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_updates_the_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Planinari Fruške gore", "Novi opis planinarskog kluba.",
            ["/api/media/images/vrh.jpg", "/api/media/images/staza.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.Hikers.Id);
        stored.Name.Should().Be("Planinari Fruške gore");
        stored.Description.Should().Be("Novi opis planinarskog kluba.");
        stored.Images.Should().Equal("/api/media/images/vrh.jpg", "/api/media/images/staza.jpg");
    }

    [Fact]
    public async Task Updating_an_unknown_club_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Nepostojeći klub", "Opis nepostojećeg kluba.", ["/api/media/images/nista.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
        assertContext.Clubs.Should().NotContain(club => club.Name == "Nepostojeći klub");
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_club()
    {
        var client = Factory.CreateClient();
        var request = new UpdateClubDto("Novo ime", "Novi opis kluba.", ["/api/media/images/novo.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateClubDto("Novo ime", "Novi opis kluba.", ["/api/media/images/novo.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_deletes_the_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Should().NotContain(club => club.Id == ClubSeed.Hikers.Id);
    }

    [Fact]
    public async Task Deleting_another_users_club_keeps_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.FortressLovers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Should().Contain(club => club.Id == ClubSeed.FortressLovers.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
