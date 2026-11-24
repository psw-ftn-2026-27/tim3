namespace Social.Domain.Clubs;

// The owner is not a role: it is the club's OwnerId, so the club always has exactly one owner.
public enum ClubRole
{
    Member,
    Moderator
}
