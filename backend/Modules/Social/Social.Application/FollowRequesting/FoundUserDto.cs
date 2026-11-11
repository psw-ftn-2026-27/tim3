namespace Social.Application.FollowRequesting;

public sealed record FoundUserDto(Guid UserId, string DisplayName, RelationDto Relation);
