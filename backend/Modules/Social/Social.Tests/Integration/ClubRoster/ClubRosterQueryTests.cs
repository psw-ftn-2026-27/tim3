using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.ClubRoster;
using Social.Application.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubRoster;

public class ClubRosterQueryTests : BaseIntegrationTest
{
    public ClubRosterQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Invitee_search_omits_the_logged_in_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var users = await client.GetFromJsonAsync<PageResult<ClubUserDto>>(
            "/api/social/clubs/invitees?text=explorer", JsonOptions);

        users!.Items.Should().NotContain(user => user.UserId == WellKnownUsers.Explorer);
        users.Items.Should().Contain(user => user.UserId == WellKnownUsers.SecondExplorer
            && user.DisplayName == "second.explorer");
    }

    [Fact]
    public async Task Owner_sees_pending_invitations_with_the_invitees_display_names()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = ClubSeed.RunnersWithAPendingInvitation;

        var invitations = await client.GetFromJsonAsync<List<ClubUserDto>>(
            $"/api/social/clubs/{seeded.Id}/invitations", JsonOptions);

        invitations!.Select(invitation => invitation.UserId)
            .Should().BeEquivalentTo(seeded.Invitations.Select(invitation => invitation.UserId));
        invitations.Should().Contain(invitation => invitation.UserId == WellKnownUsers.ThirdExplorer
            && invitation.DisplayName == "third.explorer");
    }

    [Fact]
    public async Task User_who_is_not_the_owner_sees_no_pending_invitations()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var seeded = ClubSeed.RunnersWithAPendingInvitation;

        var invitations = await client.GetFromJsonAsync<List<ClubUserDto>>(
            $"/api/social/clubs/{seeded.Id}/invitations", JsonOptions);

        invitations.Should().BeEmpty();
    }

    [Fact]
    public async Task Own_invitations_contain_only_clubs_that_invited_the_logged_in_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var expectedIds = ClubSeed.All
            .Where(club => club.Invitations.Any(invitation => invitation.UserId == WellKnownUsers.ThirdExplorer))
            .Select(club => club.Id);

        var invitations = await client.GetFromJsonAsync<List<ReceivedInvitationDto>>(
            "/api/social/clubs/invitations", JsonOptions);

        invitations!.Select(invitation => invitation.ClubId).Should().BeEquivalentTo(expectedIds);
        invitations.Should().Contain(invitation => invitation.ClubId == ClubSeed.RunnersWithAPendingInvitation.Id
            && invitation.ClubName == ClubSeed.RunnersWithAPendingInvitation.Name);
    }
}
