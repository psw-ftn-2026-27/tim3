using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class ClubMember : Entity
{
    public Guid UserId { get; private set; }
    public ClubRole Role { get; private set; }

    private ClubMember() { }

    public ClubMember(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
        Role = ClubRole.Member;
    }

    // Only Club.PromoteToModerator calls this, after checking that the caller owns the club.
    internal void Promote()
    {
        if (Role == ClubRole.Moderator)
        {
            throw new DomainException("This member is already a moderator.");
        }
        Role = ClubRole.Moderator;
    }

    // Only Club.TransferOwnership calls this, so the new owner's row carries no role of its own.
    internal void ResetToMember()
    {
        Role = ClubRole.Member;
    }
}
