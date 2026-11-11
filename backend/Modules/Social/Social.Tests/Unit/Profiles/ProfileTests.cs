using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Profiles;
using Xunit;

namespace Social.Tests.Unit.Profiles;

public class ProfileTests
{
    private const string PendingUser = "10000000-0000-0000-0000-000000000001";
    private const string FollowedUser = "10000000-0000-0000-0000-000000000002";
    private const string RejectingUser = "10000000-0000-0000-0000-000000000003";

    private static readonly Guid Sender = WellKnownUsers.Explorer;
    private static readonly Guid Recipient = WellKnownUsers.SecondExplorer;
    private static readonly DateTime Now = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Profile CreateSenderWithPendingRequest()
    {
        var sender = new Profile(Sender);
        sender.SendFollowRequest(Recipient, false, Now);
        return sender;
    }

    private static Profile CreateSenderFollowingRecipient()
    {
        var sender = CreateSenderWithPendingRequest();
        sender.AcceptFollowRequest(sender.SentRequests[0].Id, Recipient, Now);
        return sender;
    }

    private static Profile CreateSenderRejectedAt(DateTime rejectedAt)
    {
        var sender = new Profile(Sender);
        sender.SendFollowRequest(Recipient, false, rejectedAt);
        sender.RejectFollowRequest(sender.SentRequests[0].Id, Recipient, rejectedAt);
        return sender;
    }

    [Fact]
    public void User_cannot_send_a_follow_request_to_themselves()
    {
        var sender = new Profile(Sender);

        var sending = () => sender.SendFollowRequest(Sender, false, Now);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Second_pending_request_to_the_same_recipient_is_rejected()
    {
        var sender = CreateSenderWithPendingRequest();

        var sending = () => sender.SendFollowRequest(Recipient, false, Now);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_is_not_sent_to_a_user_the_sender_already_follows()
    {
        var sender = CreateSenderFollowingRecipient();

        var sending = () => sender.SendFollowRequest(Recipient, false, Now);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Already_answered_request_cannot_be_cancelled()
    {
        var sender = CreateSenderRejectedAt(Now);

        var cancellation = () => sender.CancelFollowRequest(sender.SentRequests[0].Id);

        cancellation.Should().Throw<DomainException>();
    }

    [Fact]
    public void After_cancellation_a_new_request_to_the_same_recipient_is_sent_at_once()
    {
        var sender = CreateSenderWithPendingRequest();
        sender.CancelFollowRequest(sender.SentRequests[0].Id);

        sender.SendFollowRequest(Recipient, false, Now);

        sender.SentRequests.Should().ContainSingle(request =>
            request.RecipientId == Recipient && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public void User_who_is_not_the_recipient_cannot_accept_the_request()
    {
        var sender = CreateSenderWithPendingRequest();

        var acceptance = () => sender.AcceptFollowRequest(sender.SentRequests[0].Id, WellKnownUsers.Administrator, Now);

        acceptance.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Already_rejected_request_cannot_be_accepted()
    {
        var sender = CreateSenderRejectedAt(Now);

        var acceptance = () => sender.AcceptFollowRequest(sender.SentRequests[0].Id, Recipient, Now);

        acceptance.Should().Throw<DomainException>();
    }

    [Fact]
    public void Accepted_request_means_the_sender_follows_the_recipient()
    {
        var sender = CreateSenderWithPendingRequest();

        sender.AcceptFollowRequest(sender.SentRequests[0].Id, Recipient, Now);

        sender.Follows(Recipient).Should().BeTrue();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(6.0)]
    [InlineData(6.99)]
    public void New_request_to_the_same_recipient_is_rejected_until_seven_days_have_passed_since_the_rejection(double daysAfterRejection)
    {
        var sender = CreateSenderRejectedAt(Now);

        var sending = () => sender.SendFollowRequest(Recipient, false, Now.AddDays(daysAfterRejection));

        sending.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(7.0)]
    [InlineData(8.0)]
    public void New_request_to_the_same_recipient_is_sent_once_seven_days_have_passed_since_the_rejection(double daysAfterRejection)
    {
        var sender = CreateSenderRejectedAt(Now);

        sender.SendFollowRequest(Recipient, false, Now.AddDays(daysAfterRejection));

        sender.SentRequests.Should().ContainSingle(request =>
            request.RecipientId == Recipient && request.Status == FollowRequestStatus.Pending);
    }

    [Theory]
    [InlineData(PendingUser, RelationKind.Pending, true, false)]
    [InlineData(FollowedUser, RelationKind.Following, false, false)]
    [InlineData(RejectingUser, RelationKind.BlockedUntil, false, true)]
    public void Relation_to_another_user_shows_the_pending_request_the_following_and_when_sending_is_possible_again(
        string otherUser, RelationKind expectedKind, bool carriesRequestId, bool carriesAvailableFrom)
    {
        var profile = new Profile(Sender);
        profile.SendFollowRequest(Guid.Parse(PendingUser), false, Now);
        profile.SendFollowRequest(Guid.Parse(FollowedUser), true, Now);
        profile.SendFollowRequest(Guid.Parse(RejectingUser), false, Now);
        profile.RejectFollowRequest(profile.SentRequests[2].Id, Guid.Parse(RejectingUser), Now);

        var relation = profile.RelationTo(Guid.Parse(otherUser), Now);

        relation.Kind.Should().Be(expectedKind);
        relation.RequestId.Should().Be(carriesRequestId ? profile.SentRequests[0].Id : null);
        relation.AvailableFrom.Should().Be(carriesAvailableFrom ? Now.AddDays(7) : null);
    }
}
