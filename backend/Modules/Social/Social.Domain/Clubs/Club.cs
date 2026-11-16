using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    private const int MaxImages = 5;

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Images { get; private set; }
    private readonly List<ClubMember> _members = [];
    public IReadOnlyList<ClubMember> Members => _members;
    private readonly List<JoinRequest> _joinRequests = [];
    public IReadOnlyList<JoinRequest> JoinRequests => _joinRequests;
    private readonly List<ClubInvitation> _invitations = [];
    public IReadOnlyList<ClubInvitation> Invitations => _invitations;

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string> images)
        : base(Guid.NewGuid())
    {
        EnsureValid(name, description, images);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        Images = images;

        // P7: the owner is a member from the founding.
        _members.Add(new ClubMember(ownerId));
    }

    public void Update(Guid userId, string name, string description, List<string> images)
    {
        EnsureOwnedBy(userId);
        EnsureValid(name, description, images);

        Name = name;
        Description = description;
        Images = images;
    }

    public void EnsureCanBeDeletedBy(Guid userId)
    {
        EnsureOwnedBy(userId);
    }

    public void RequestToJoin(Guid userId)
    {
        if (_members.Any(member => member.UserId == userId))
        {
            throw new DomainException("You are already a member of this club.");
        }
        EnsureNoOpenRequestOrInvitation(userId, askedByOwner: false);

        _joinRequests.Add(new JoinRequest(userId));
    }

    // P3: a user can withdraw only the request they sent, so the request is found by their ID.
    public void WithdrawRequest(Guid userId)
    {
        var request = GetRequestFrom(userId);

        _joinRequests.Remove(request);
    }

    public void AcceptRequest(Guid userId, Guid requesterId)
    {
        EnsureOwnedBy(userId);
        var request = GetRequestFrom(requesterId);

        _joinRequests.Remove(request);
        _members.Add(new ClubMember(requesterId));
    }

    public void RejectRequest(Guid userId, Guid requesterId)
    {
        EnsureOwnedBy(userId);
        var request = GetRequestFrom(requesterId);

        _joinRequests.Remove(request);
    }

    public void Invite(Guid userId, Guid inviteeId)
    {
        EnsureOwnedBy(userId);
        if (_members.Any(member => member.UserId == inviteeId))
        {
            throw new DomainException("This user is already a member of the club.");
        }
        EnsureNoOpenRequestOrInvitation(inviteeId, askedByOwner: true);

        _invitations.Add(new ClubInvitation(inviteeId));
    }

    public void WithdrawInvitation(Guid userId, Guid inviteeId)
    {
        EnsureOwnedBy(userId);
        var invitation = GetInvitationFor(inviteeId);

        _invitations.Remove(invitation);
    }

    // P5: only the invited user can answer an invitation, so it is found by their ID.
    public void AcceptInvitation(Guid userId)
    {
        var invitation = GetInvitationFor(userId);

        _invitations.Remove(invitation);
        _members.Add(new ClubMember(userId));
    }

    public void DeclineInvitation(Guid userId)
    {
        var invitation = GetInvitationFor(userId);

        _invitations.Remove(invitation);
    }

    public void RemoveMember(Guid userId, Guid memberId)
    {
        EnsureOwnedBy(userId);
        if (memberId == OwnerId)
        {
            throw new DomainException("The owner cannot remove themselves from the club.");
        }
        var member = _members.FirstOrDefault(member => member.UserId == memberId);
        if (member is null)
        {
            throw new NotFoundException("This user is not a member of the club.");
        }

        _members.Remove(member);
    }

    // P3: one user and one club have at most one open join request or invitation at a time.
    // The condition is the same for both callers; only the message depends on who reads it.
    private void EnsureNoOpenRequestOrInvitation(Guid userId, bool askedByOwner)
    {
        if (_joinRequests.Any(request => request.UserId == userId))
        {
            throw new DomainException(askedByOwner
                ? "This user already has a pending join request. Resolve it under Join requests below."
                : "Your request to join this club is already pending.");
        }
        if (_invitations.Any(invitation => invitation.UserId == userId))
        {
            throw new DomainException(askedByOwner
                ? "This user already has a pending invitation. You can withdraw it under Pending invitations."
                : "You already have an invitation to this club. Answer it on My club invitations.");
        }
    }

    private ClubInvitation GetInvitationFor(Guid userId)
    {
        var invitation = _invitations.FirstOrDefault(invitation => invitation.UserId == userId);
        if (invitation is null)
        {
            throw new NotFoundException("This invitation no longer exists.");
        }
        return invitation;
    }

    private JoinRequest GetRequestFrom(Guid userId)
    {
        var request = _joinRequests.FirstOrDefault(request => request.UserId == userId);
        if (request is null)
        {
            throw new NotFoundException("This join request no longer exists.");
        }
        return request;
    }

    private void EnsureOwnedBy(Guid userId)
    {
        if (OwnerId != userId)
        {
            throw new NotFoundException($"Club {Id} does not exist.");
        }
    }

    private static void EnsureValid(string name, string description, List<string> images)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images.Count < 1 || images.Count > MaxImages)
        {
            throw new DomainException($"A club requires between one and {MaxImages} images.");
        }
    }
}
