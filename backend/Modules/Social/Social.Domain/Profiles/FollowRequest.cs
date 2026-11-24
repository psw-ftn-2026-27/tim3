using Shared.Domain;

namespace Social.Domain.Profiles;

public sealed class FollowRequest : Entity
{
    public Guid RecipientId { get; private set; }
    public DateTime SentAt { get; private set; }
    public FollowRequestStatus Status { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    private FollowRequest() { }

    public FollowRequest(Guid recipientId, DateTime sentAt) : base(Guid.NewGuid())
    {
        RecipientId = recipientId;
        SentAt = sentAt;
        Status = FollowRequestStatus.Pending;
    }

    public void Accept(DateTime respondedAt)
    {
        Status = FollowRequestStatus.Accepted;
        RespondedAt = respondedAt;
    }

    public void Reject(DateTime respondedAt)
    {
        Status = FollowRequestStatus.Rejected;
        RespondedAt = respondedAt;
    }
}
