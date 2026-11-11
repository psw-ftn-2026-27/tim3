namespace Social.Domain.Profiles;

public static class FollowerRemoval
{
    // P1, P3: ends only the follower's following of the remover and remembers the removal.
    // The follower's profile is changed first, because only it can tell whether the following
    // exists; a rejection therefore leaves both profiles untouched.
    public static void Remove(Profile remover, Profile follower)
    {
        follower.RemoveAsFollowerOf(remover.UserId);
        remover.RememberRemovedFollower(follower.UserId);
    }
}
