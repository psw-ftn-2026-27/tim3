namespace Social.Domain.Clubs;

public sealed record ClubInvitation
{
    public Guid UserId { get; private init; }

    private ClubInvitation() { }

    public ClubInvitation(Guid userId)
    {
        UserId = userId;
    }
}
