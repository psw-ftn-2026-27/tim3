using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubRoles;

public class ClubRolesCommandTests : BaseIntegrationTest
{
    public ClubRolesCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_promotes_a_member_and_the_moderator_role_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().Contain(member => member.UserId == WellKnownUsers.FourthExplorer
            && member.Role == ClubRole.Moderator);
    }

    [Fact]
    public async Task Promoting_in_another_users_club_keeps_the_members_role()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().Contain(member => member.UserId == WellKnownUsers.FourthExplorer
            && member.Role == ClubRole.Member);
    }

    [Fact]
    public async Task Anonymous_user_cannot_promote_a_member()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_promote_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_transfers_ownership_the_member_is_saved_as_owner_and_the_former_owner_is_a_regular_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.OwnerId.Should().Be(WellKnownUsers.FourthExplorer);
        stored.Members.Should().Contain(member => member.UserId == WellKnownUsers.Explorer
            && member.Role == ClubRole.Member);
    }

    [Fact]
    public async Task Transferring_ownership_in_another_users_club_keeps_the_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_transfer_ownership()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_transfer_ownership()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
