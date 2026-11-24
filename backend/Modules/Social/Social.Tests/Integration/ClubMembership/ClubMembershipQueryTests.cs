using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubMembership;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipQueryTests : BaseIntegrationTest
{
    public ClubMembershipQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Club_view_shows_members_with_display_names_and_the_logged_in_users_state()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var seeded = ClubSeed.CyclistsWithAMemberAndAPendingRequest;

        var club = await client.GetFromJsonAsync<ClubDetailsDto>($"/api/social/clubs/{seeded.Id}", JsonOptions);

        club!.Id.Should().Be(seeded.Id);
        club.Members.Select(member => member.UserId).Should().BeEquivalentTo(seeded.Members.Select(member => member.UserId));
        club.Members.Should().Contain(member => member.UserId == WellKnownUsers.Explorer && member.DisplayName == "explorer");
        club.Members.Should().Contain(member => member.UserId == WellKnownUsers.FourthExplorer && member.DisplayName == "fourth.explorer");
        club.Membership.Should().Be(MembershipState.Pending);
    }

    [Fact]
    public async Task Owner_sees_pending_requests_with_the_requesters_display_names()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = ClubSeed.CyclistsWithAMemberAndAPendingRequest;

        var club = await client.GetFromJsonAsync<ClubDetailsDto>($"/api/social/clubs/{seeded.Id}", JsonOptions);

        club!.PendingRequests.Select(request => request.UserId)
            .Should().BeEquivalentTo(seeded.JoinRequests.Select(request => request.UserId));
        club.PendingRequests.Should().Contain(request => request.UserId == WellKnownUsers.ThirdExplorer
            && request.DisplayName == "third.explorer");
    }

    [Fact]
    public async Task Club_view_shows_members_with_roles_the_owner_from_the_clubs_ownership_and_the_logged_in_users_role()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.FourthExplorer, "explorer");
        var seeded = ClubSeed.HistoriansWithAModeratorAMemberAndAPendingRequest;

        var club = await client.GetFromJsonAsync<ClubDetailsDto>($"/api/social/clubs/{seeded.Id}", JsonOptions);

        club!.Members.Select(member => member.UserId).Should().BeEquivalentTo(seeded.Members.Select(member => member.UserId));
        club.Members.Should().Contain(member => member.UserId == seeded.OwnerId && member.Role == MemberRole.Owner);
        club.Members.Should().Contain(member => member.UserId == WellKnownUsers.ThirdExplorer && member.Role == MemberRole.Moderator);
        club.Members.Should().Contain(member => member.UserId == WellKnownUsers.FourthExplorer && member.Role == MemberRole.Member);
        club.Role.Should().Be(MemberRole.Member);
    }

    [Fact]
    public async Task User_who_is_not_a_member_has_no_role_in_the_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = ClubSeed.HistoriansWithAModeratorAMemberAndAPendingRequest;

        var club = await client.GetFromJsonAsync<ClubDetailsDto>($"/api/social/clubs/{seeded.Id}", JsonOptions);

        club!.Role.Should().BeNull();
    }

    [Fact]
    public async Task Moderator_sees_pending_requests_with_the_requesters_display_names()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var seeded = ClubSeed.HistoriansWithAModeratorAMemberAndAPendingRequest;

        var club = await client.GetFromJsonAsync<ClubDetailsDto>($"/api/social/clubs/{seeded.Id}", JsonOptions);

        club!.Role.Should().Be(MemberRole.Moderator);
        club.PendingRequests.Select(request => request.UserId)
            .Should().BeEquivalentTo(seeded.JoinRequests.Select(request => request.UserId));
        club.PendingRequests.Should().Contain(request => request.UserId == WellKnownUsers.Explorer
            && request.DisplayName == "explorer");
    }

    [Fact]
    public async Task Regular_member_sees_no_pending_requests()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.FourthExplorer, "explorer");
        var seeded = ClubSeed.HistoriansWithAModeratorAMemberAndAPendingRequest;

        var club = await client.GetFromJsonAsync<ClubDetailsDto>($"/api/social/clubs/{seeded.Id}", JsonOptions);

        club!.PendingRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task Viewing_a_club_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.GetAsync($"/api/social/clubs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
