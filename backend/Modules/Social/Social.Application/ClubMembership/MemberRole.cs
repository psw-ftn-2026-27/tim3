namespace Social.Application.ClubMembership;

// A member's role as the club view shows it. Owner comes from the club's OwnerId, the other
// two from the member's own row.
public enum MemberRole
{
    Owner,
    Moderator,
    Member
}
