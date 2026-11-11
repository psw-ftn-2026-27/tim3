using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Profiles;
using Xunit;

namespace Social.Tests.Unit.Profiles;

public class FollowRequestSenderTests
{
    private static readonly Guid Sender = WellKnownUsers.Explorer;
    private static readonly Guid Recipient = WellKnownUsers.SecondExplorer;
    private static readonly DateTime Now = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Profile CreateRecipientFollowingSender()
    {
        var recipient = new Profile(Recipient);
        recipient.SendFollowRequest(Sender, false, Now.AddDays(-30));
        recipient.AcceptFollowRequest(recipient.SentRequests[0].Id, Sender, Now.AddDays(-30));
        return recipient;
    }

    [Fact]
    public void Request_to_a_user_who_already_follows_the_sender_becomes_following_at_once()
    {
        var sender = new Profile(Sender);
        var recipient = CreateRecipientFollowingSender();

        FollowRequestSender.Send(sender, recipient, Now);

        sender.Follows(Recipient).Should().BeTrue();
    }

    [Fact]
    public void Request_to_a_user_who_only_sent_the_sender_a_request_stays_pending()
    {
        var sender = new Profile(Sender);
        var recipient = new Profile(Recipient);
        recipient.SendFollowRequest(Sender, false, Now.AddDays(-1));

        FollowRequestSender.Send(sender, recipient, Now);

        sender.SentRequests.Should().ContainSingle(request =>
            request.RecipientId == Recipient && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public void Rejection_less_than_seven_days_ago_blocks_the_request_even_when_the_recipient_follows_the_sender()
    {
        var sender = new Profile(Sender);
        sender.SendFollowRequest(Recipient, false, Now.AddDays(-3));
        sender.RejectFollowRequest(sender.SentRequests[0].Id, Recipient, Now.AddDays(-3));
        var recipient = CreateRecipientFollowingSender();

        var sending = () => FollowRequestSender.Send(sender, recipient, Now);

        sending.Should().Throw<DomainException>();
    }
}
