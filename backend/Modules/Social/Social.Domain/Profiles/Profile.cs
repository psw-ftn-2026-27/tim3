using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Profiles;

public sealed class Profile : AggregateRoot
{
    private const int DaysBlockedAfterRejection = 7;

    public Guid UserId { get; private set; }
    public List<Guid> Following { get; private set; }
    public List<Guid> RemovedFollowers { get; private set; }
    private readonly List<FollowRequest> _sentRequests = [];
    public IReadOnlyList<FollowRequest> SentRequests => _sentRequests;

    private Profile()
    {
        Following = null!;
        RemovedFollowers = null!;
    }

    public Profile(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
        Following = [];
        RemovedFollowers = [];
    }

    public bool Follows(Guid userId)
    {
        return Following.Contains(userId);
    }

    public Relation RelationTo(Guid otherUserId, DateTime now)
    {
        if (otherUserId == UserId)
        {
            return new Relation(RelationKind.Self, null, null);
        }
        if (Follows(otherUserId))
        {
            return new Relation(RelationKind.Following, null, null);
        }

        var pending = _sentRequests.FirstOrDefault(request =>
            request.RecipientId == otherUserId && request.Status == FollowRequestStatus.Pending);
        if (pending is not null)
        {
            return new Relation(RelationKind.Pending, pending.Id, null);
        }

        var availableFrom = SendingAvailableFrom(otherUserId);
        if (availableFrom is not null && now < availableFrom)
        {
            return new Relation(RelationKind.BlockedUntil, null, availableFrom);
        }

        return new Relation(RelationKind.CanFollow, null, null);
    }

    // P8: a new request to a recipient is possible seven days after their latest rejection.
    private DateTime? SendingAvailableFrom(Guid recipientId)
    {
        var latestRejection = _sentRequests
            .Where(request => request.RecipientId == recipientId && request.Status == FollowRequestStatus.Rejected)
            .MaxBy(request => request.RespondedAt);

        return latestRejection?.RespondedAt!.Value.AddDays(DaysBlockedAfterRejection);
    }

    public void SendFollowRequest(Guid recipientId, bool recipientAcceptsImmediately, DateTime now)
    {
        var relation = RelationTo(recipientId, now);
        switch (relation.Kind)
        {
            case RelationKind.Self:
                throw new DomainException("You cannot send a follow request to yourself.");
            case RelationKind.Pending:
                throw new DomainException("A follow request to this user is already pending.");
            case RelationKind.Following:
                throw new DomainException("You already follow this user.");
            case RelationKind.BlockedUntil:
                throw new DomainException($"You can send a new follow request to this user from {relation.AvailableFrom:yyyy-MM-dd HH:mm} UTC.");
        }

        var request = new FollowRequest(recipientId, now);
        if (recipientAcceptsImmediately)
        {
            request.Accept(now);
            Following.Add(recipientId);
        }
        _sentRequests.Add(request);
    }

    public void CancelFollowRequest(Guid requestId)
    {
        var request = GetSentRequest(requestId);
        EnsurePending(request);

        _sentRequests.Remove(request);
    }

    public void AcceptFollowRequest(Guid requestId, Guid recipientId, DateTime now)
    {
        var request = GetRequestSentTo(requestId, recipientId);
        EnsurePending(request);

        request.Accept(now);
        Following.Add(recipientId);
    }

    public void RejectFollowRequest(Guid requestId, Guid recipientId, DateTime now)
    {
        var request = GetRequestSentTo(requestId, recipientId);
        EnsurePending(request);

        request.Reject(now);
    }

    public void Unfollow(Guid followedId)
    {
        StopFollowing(followedId);
    }

    // Called on the follower's profile when the followed user removes them from their followers.
    public void RemoveAsFollowerOf(Guid removerId)
    {
        StopFollowing(removerId);
    }

    public void RememberRemovedFollower(Guid followerId)
    {
        if (!RemovedFollowers.Contains(followerId))
        {
            RemovedFollowers.Add(followerId);
        }
    }

    // P3: only the two sides of a following can end it, so the following has to exist here.
    private void StopFollowing(Guid followedId)
    {
        if (!Follows(followedId))
        {
            throw new DomainException("This following does not exist.");
        }

        Following.Remove(followedId);
    }

    private FollowRequest GetSentRequest(Guid requestId)
    {
        var request = _sentRequests.FirstOrDefault(request => request.Id == requestId);
        if (request is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }
        return request;
    }

    private FollowRequest GetRequestSentTo(Guid requestId, Guid recipientId)
    {
        var request = GetSentRequest(requestId);
        if (request.RecipientId != recipientId)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }
        return request;
    }

    private static void EnsurePending(FollowRequest request)
    {
        if (request.Status != FollowRequestStatus.Pending)
        {
            throw new DomainException("The follow request has already been answered.");
        }
    }
}
