using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.FollowRequesting;
using Social.Domain.Profiles;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequesting;

public class FollowRequestingQueryTests : BaseIntegrationTest
{
    public FollowRequestingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Incoming_requests_contain_only_pending_requests_to_the_logged_in_user_with_the_senders_display_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var expectedIds = ProfileSeed.All
            .SelectMany(profile => profile.SentRequests)
            .Where(sent => sent.RecipientId == WellKnownUsers.SecondExplorer && sent.Status == FollowRequestStatus.Pending)
            .Select(sent => sent.Id);
        var explorersRequestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var incoming = await client.GetFromJsonAsync<PageResult<IncomingFollowRequestDto>>(
            "/api/social/follow-requests/incoming", JsonOptions);

        incoming!.Items.Select(request => request.Id).Should().BeEquivalentTo(expectedIds);
        incoming.Items.Should().Contain(request => request.Id == explorersRequestId
            && request.SenderId == WellKnownUsers.Explorer
            && request.SenderDisplayName == "explorer");
    }

    [Fact]
    public async Task Search_returns_matching_users_with_display_name_and_relation_and_omits_the_logged_in_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var explorersRequestId = ProfileSeed.ExplorerPendingToSecondExplorer.SentRequests.Single().Id;

        var users = await client.GetFromJsonAsync<PageResult<FoundUserDto>>(
            "/api/social/users?text=explorer", JsonOptions);

        users!.Items.Should().NotContain(user => user.UserId == WellKnownUsers.Explorer);
        users.Items.Should().Contain(user => user.UserId == WellKnownUsers.SecondExplorer
            && user.DisplayName == "second.explorer"
            && user.Relation.Kind == RelationKind.Pending
            && user.Relation.RequestId == explorersRequestId);
    }
}
