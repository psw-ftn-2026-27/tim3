using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubRoster;

public class ClubRosterCommandTests : BaseIntegrationTest
{
    public ClubRosterCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_invites_an_explorer_and_the_invitation_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.Hikers.Id}/invitations/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.Hikers.Id);
        stored.Invitations.Should().Contain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Invitation_to_a_member_of_the_club_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/invitations/{WellKnownUsers.FourthExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.FourthExplorer);
    }

    [Fact]
    public async Task Invitation_to_a_user_who_does_not_exist_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var invitationCountBefore = CountInvitations(arrangeContext);

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.Hikers.Id}/invitations/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        CountInvitations(assertContext).Should().Be(invitationCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_invite_to_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.Hikers.Id}/invitations/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_invite_to_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.Hikers.Id}/invitations/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_withdraws_the_invitation_and_it_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/invitations/{WellKnownUsers.ThirdExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_an_invitation()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/invitations/{WellKnownUsers.ThirdExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_withdraw_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/invitations/{WellKnownUsers.ThirdExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invitee_accepts_the_invitation_becomes_a_member_and_the_invitation_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/invitations/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().Contain(member => member.UserId == WellKnownUsers.ThirdExplorer);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_an_invitation()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/invitations/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/invitations/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invitee_declines_the_invitation_and_it_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/invitations/decline", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.ThirdExplorer);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_decline_an_invitation()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/invitations/decline", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_decline_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.RunnersWithAPendingInvitation.Id;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/invitations/decline", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_removes_a_member_and_the_user_is_no_longer_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.FourthExplorer);
    }

    [Fact]
    public async Task Removing_a_member_in_another_users_club_keeps_the_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().Contain(member => member.UserId == WellKnownUsers.FourthExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_member()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.FourthExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static int CountInvitations(SocialDbContext context)
    {
        return context.Clubs.AsEnumerable().Sum(club => club.Invitations.Count);
    }
}
