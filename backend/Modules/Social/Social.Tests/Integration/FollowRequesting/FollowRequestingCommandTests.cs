using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.FollowRequesting;
using Social.Domain.Profiles;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequesting;

public class FollowRequestingCommandTests : BaseIntegrationTest
{
    public FollowRequestingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_without_a_profile_sends_a_request_to_a_user_without_a_profile_and_it_is_saved_as_pending()
    {
        var senderId = Guid.NewGuid();
        var client = Factory.CreateClientFor(senderId, "explorer");
        var request = new SendFollowRequestDto(WellKnownUsers.SecondExplorer);

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Profiles.Single(profile => profile.UserId == senderId);
        stored.SentRequests.Should().ContainSingle(sent =>
            sent.RecipientId == WellKnownUsers.SecondExplorer && sent.Status == FollowRequestStatus.Pending);
        assertContext.Profiles.Should().NotContain(profile => profile.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task User_cannot_send_a_follow_request_to_themselves_and_nothing_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new SendFollowRequestDto(WellKnownUsers.Explorer);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Profiles.Single(profile => profile.UserId == WellKnownUsers.Explorer).SentRequests.Count;

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Profiles.Single(profile => profile.UserId == WellKnownUsers.Explorer)
            .SentRequests.Count.Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Request_to_a_nonexistent_user_is_rejected_and_nothing_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new SendFollowRequestDto(Guid.NewGuid());
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Profiles.Single(profile => profile.UserId == WellKnownUsers.Explorer).SentRequests.Count;

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Profiles.Single(profile => profile.UserId == WellKnownUsers.Explorer)
            .SentRequests.Count.Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_a_follow_request()
    {
        var client = Factory.CreateClient();
        var request = new SendFollowRequestDto(WellKnownUsers.SecondExplorer);

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_send_a_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new SendFollowRequestDto(WellKnownUsers.SecondExplorer);

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Sender_cancels_their_pending_request_and_it_is_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.DeleteAsync($"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Profiles.Single(profile => profile.Id == ProfileSeed.ExplorerPendingToSecondExplorer.Id);
        stored.SentRequests.Should().NotContain(sent => sent.Id == requestId);
    }

    [Fact]
    public async Task User_cannot_cancel_another_users_request_and_it_stays_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.DeleteAsync($"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Profiles.Single(profile => profile.Id == ProfileSeed.ExplorerPendingToSecondExplorer.Id);
        stored.SentRequests.Should().Contain(sent => sent.Id == requestId && sent.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Anonymous_user_cannot_cancel_a_follow_request()
    {
        var client = Factory.CreateClient();
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.DeleteAsync($"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_cancel_a_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.DeleteAsync($"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Recipient_accepts_the_request_and_the_sender_follows_them_from_then_on()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Profiles.Single(profile => profile.Id == ProfileSeed.ExplorerPendingToSecondExplorer.Id);
        stored.Follows(WellKnownUsers.SecondExplorer).Should().BeTrue();
        stored.SentRequests.Single(sent => sent.Id == requestId).Status.Should().Be(FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task Unknown_request_cannot_be_accepted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.PostAsync($"/api/social/follow-requests/{Guid.NewGuid()}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Profiles.Single(profile => profile.Id == ProfileSeed.ExplorerPendingToSecondExplorer.Id);
        stored.Follows(WellKnownUsers.SecondExplorer).Should().BeFalse();
        stored.SentRequests.Single(sent => sent.Id == requestId).Status.Should().Be(FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_follow_request()
    {
        var client = Factory.CreateClient();
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_a_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Recipient_rejects_the_request_and_it_is_saved_as_rejected_with_the_time_of_rejection()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Profiles.Single(profile => profile.Id == ProfileSeed.ExplorerPendingToSecondExplorer.Id);
        var rejected = stored.SentRequests.Single(sent => sent.Id == requestId);
        rejected.Status.Should().Be(FollowRequestStatus.Rejected);
        rejected.RespondedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_follow_request()
    {
        var client = Factory.CreateClient();
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reject_a_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var requestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
