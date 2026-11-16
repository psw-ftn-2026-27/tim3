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
        if (_joinRequests.Any(request => request.UserId == userId))
        {
            throw new DomainException("Your request to join this club is already pending.");
        }

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
