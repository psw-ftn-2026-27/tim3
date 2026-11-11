using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowManagement;

public class FollowManagementCommandTests : BaseIntegrationTest
{
    public FollowManagementCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_without_a_profile_removes_a_follower_who_then_no_longer_follows_them_and_the_removal_is_remembered_in_a_new_profile()
    {
        var follower = ProfileSeed.FollowingAUserWithoutAProfile;
        var removerId = follower.Following.Single();
        var client = Factory.CreateClientFor(removerId, "explorer");

        var response = await client.DeleteAsync($"/api/social/profile/followers/{follower.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Profiles.Single(profile => profile.Id == follower.Id)
            .Follows(removerId).Should().BeFalse();
        assertContext.Profiles.Single(profile => profile.UserId == removerId)
            .RemovedFollowers.Should().Contain(follower.UserId);
    }

    [Fact]
    public async Task User_cannot_remove_a_user_who_does_not_follow_them_and_nothing_is_saved()
    {
        var remover = ProfileSeed.FourthExplorerFollowingThirdExplorerAndAUserWhoDoesNotFollowBack;
        var notFollowerId = remover.Following.Single(userId => userId != WellKnownUsers.ThirdExplorer);
        var client = Factory.CreateClientFor(remover.UserId, "explorer");

        var response = await client.DeleteAsync($"/api/social/profile/followers/{notFollowerId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Profiles.Single(profile => profile.Id == remover.Id);
        stored.RemovedFollowers.Should().NotContain(notFollowerId);
        stored.Follows(notFollowerId).Should().BeTrue();
        assertContext.Profiles.Should().NotContain(profile => profile.UserId == notFollowerId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_follower()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/profile/followers/{WellKnownUsers.ThirdExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_follower()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/profile/followers/{WellKnownUsers.ThirdExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task User_unfollows_a_user_and_the_following_is_deleted()
    {
        var profile = ProfileSeed.ThirdExplorerFollowingFourthExplorerAndAnotherUser;
        var client = Factory.CreateClientFor(profile.UserId, "explorer");

        var response = await client.DeleteAsync($"/api/social/profile/following/{WellKnownUsers.FourthExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Profiles.Single(stored => stored.Id == profile.Id)
            .Follows(WellKnownUsers.FourthExplorer).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_unfollow()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/profile/following/{WellKnownUsers.FourthExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_unfollow()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/profile/following/{WellKnownUsers.FourthExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
