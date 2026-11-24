using Social.Domain.Profiles;

namespace Social.Application.FollowRequesting;

public sealed record RelationDto(RelationKind Kind, Guid? RequestId, DateTime? AvailableFrom);
