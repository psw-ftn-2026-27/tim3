namespace Social.Domain.Profiles;

public static class FollowRequestSender
{
    // P4: a recipient who follows the sender accepts at once, unless the recipient has removed
    // the sender from their followers. A pending request from the recipient is not following.
    public static void Send(Profile sender, Profile recipient, DateTime now)
    {
        var recipientAcceptsImmediately = recipient.Follows(sender.UserId)
            && !recipient.RemovedFollowers.Contains(sender.UserId);

        sender.SendFollowRequest(recipient.UserId, recipientAcceptsImmediately, now);
    }
}
