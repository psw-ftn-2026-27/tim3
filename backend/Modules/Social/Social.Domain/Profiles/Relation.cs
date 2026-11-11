namespace Social.Domain.Profiles;

public sealed record Relation(RelationKind Kind, Guid? RequestId, DateTime? AvailableFrom);
