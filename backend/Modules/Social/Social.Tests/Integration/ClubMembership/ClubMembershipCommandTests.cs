using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipCommandTests : BaseIntegrationTest
{
    public ClubMembershipCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_requests_to_join_a_club_and_the_request_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.Hikers.Id);
        stored.JoinRequests.Should().Contain(request => request.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Request_to_join_from_a_member_of_the_club_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.FourthExplorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.FourthExplorer);
    }

    [Fact]
    public async Task Request_to_join_an_unknown_club_is_not_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = CountJoinRequests(arrangeContext);

        var response = await client.PostAsync($"/api/social/clubs/{Guid.NewGuid()}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        CountJoinRequests(assertContext).Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_request_to_join_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_request_to_join_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.Hikers.Id}/join-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task User_withdraws_the_request_and_it_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.DeleteAsync($"/api/social/clubs/{clubId}/join-requests");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.DeleteAsync($"/api/social/clubs/{clubId}/join-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.DeleteAsync($"/api/social/clubs/{clubId}/join-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_accepts_the_request_the_requester_becomes_a_member_and_the_request_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/join-requests/{WellKnownUsers.ThirdExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().Contain(member => member.UserId == WellKnownUsers.ThirdExplorer);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Accepting_a_request_in_another_users_club_keeps_the_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/join-requests/{WellKnownUsers.ThirdExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.JoinRequests.Should().Contain(request => request.UserId == WellKnownUsers.ThirdExplorer);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_request()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/join-requests/{WellKnownUsers.ThirdExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/join-requests/{WellKnownUsers.ThirdExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_rejects_the_request_it_no_longer_exists_and_the_requester_is_not_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/join-requests/{WellKnownUsers.ThirdExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.ThirdExplorer);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_request()
    {
        var client = Factory.CreateClient();
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/join-requests/{WellKnownUsers.ThirdExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reject_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var clubId = ClubSeed.CyclistsWithAMemberAndAPendingRequest.Id;

        var response = await client.PostAsync(
            $"/api/social/clubs/{clubId}/join-requests/{WellKnownUsers.ThirdExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static int CountJoinRequests(SocialDbContext context)
    {
        return context.Clubs.AsEnumerable().Sum(club => club.JoinRequests.Count);
    }
}
