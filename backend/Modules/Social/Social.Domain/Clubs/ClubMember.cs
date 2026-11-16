using Shared.Domain;

namespace Social.Domain.Clubs;

public sealed class ClubMember : Entity
{
    public Guid UserId { get; private set; }

    private ClubMember() { }

    public ClubMember(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }
}
