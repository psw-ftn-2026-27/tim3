namespace Social.Domain.Profiles;

public enum RelationKind
{
    CanFollow,
    Pending,
    Following,
    BlockedUntil,
    Self
}
