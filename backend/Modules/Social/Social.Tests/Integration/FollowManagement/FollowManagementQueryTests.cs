using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.FollowManagement;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowManagement;

public class FollowManagementQueryTests : BaseIntegrationTest
{
    public FollowManagementQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Followers_contain_the_users_who_follow_the_logged_in_user_with_display_name_and_not_a_user_they_only_follow()
    {
        var profile = ProfileSeed.FourthExplorerFollowingThirdExplorerAndAUserWhoDoesNotFollowBack;
        var notFollowerId = profile.Following.Single(userId => userId != WellKnownUsers.ThirdExplorer);
        var client = Factory.CreateClientFor(profile.UserId, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<ConnectedUserDto>>(
            "/api/social/profile/followers", JsonOptions);

        followers!.Items.Should().Contain(user => user.UserId == WellKnownUsers.ThirdExplorer
            && user.DisplayName == "third.explorer");
        followers.Items.Should().NotContain(user => user.UserId == notFollowerId);
    }

    [Fact]
    public async Task Following_contains_the_users_the_logged_in_user_follows_with_display_name()
    {
        var profile = ProfileSeed.ThirdExplorerFollowingFourthExplorerAndAnotherUser;
        var client = Factory.CreateClientFor(profile.UserId, "explorer");

        var following = await client.GetFromJsonAsync<PageResult<ConnectedUserDto>>(
            "/api/social/profile/following", JsonOptions);

        following!.Items.Select(user => user.UserId).Should().BeEquivalentTo(profile.Following);
        following.Items.Should().Contain(user => user.UserId == WellKnownUsers.FourthExplorer
            && user.DisplayName == "fourth.explorer");
    }

    [Fact]
    public async Task Profile_counts_of_a_user_with_different_numbers_of_followers_and_followed_users_match_the_seeded_followings()
    {
        var userId = WellKnownUsers.ThirdExplorer;
        var expectedFollowerCount = ProfileSeed.All.Count(profile => profile.Follows(userId));
        var expectedFollowingCount = ProfileSeed.All.Single(profile => profile.UserId == userId).Following.Count;
        var client = Factory.CreateClientFor(userId, "explorer");

        var summary = await client.GetFromJsonAsync<ProfileSummaryDto>("/api/social/profile/summary", JsonOptions);

        summary.Should().Be(new ProfileSummaryDto(expectedFollowerCount, expectedFollowingCount));
    }
}
