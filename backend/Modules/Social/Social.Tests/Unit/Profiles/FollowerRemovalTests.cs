using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Profiles;
using Xunit;

namespace Social.Tests.Unit.Profiles;

public class FollowerRemovalTests
{
    private static readonly Guid Remover = WellKnownUsers.Explorer;
    private static readonly Guid Follower = WellKnownUsers.SecondExplorer;
    private static readonly DateTime Now = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Profile CreateFollowerOfRemover()
    {
        var follower = new Profile(Follower);
        follower.SendFollowRequest(Remover, true, Now);
        return follower;
    }

    private static Profile CreateRemoverFollowingFollower()
    {
        var remover = new Profile(Remover);
        remover.SendFollowRequest(Follower, true, Now);
        return remover;
    }

    [Fact]
    public void Removed_follower_no_longer_follows_the_user_while_the_user_still_follows_them()
    {
        var remover = CreateRemoverFollowingFollower();
        var follower = CreateFollowerOfRemover();

        FollowerRemoval.Remove(remover, follower);

        follower.Follows(Remover).Should().BeFalse();
        remover.Follows(Follower).Should().BeTrue();
    }

    [Fact]
    public void Removed_follower_is_remembered_by_the_user_who_removed_them()
    {
        var remover = new Profile(Remover);
        var follower = CreateFollowerOfRemover();

        FollowerRemoval.Remove(remover, follower);

        remover.RemovedFollowers.Should().Contain(Follower);
    }

    [Fact]
    public void User_who_does_not_follow_the_remover_cannot_be_removed_and_nothing_is_remembered()
    {
        var remover = CreateRemoverFollowingFollower();
        var follower = new Profile(Follower);

        var removal = () => FollowerRemoval.Remove(remover, follower);

        removal.Should().Throw<DomainException>();
        remover.RemovedFollowers.Should().BeEmpty();
    }
}
