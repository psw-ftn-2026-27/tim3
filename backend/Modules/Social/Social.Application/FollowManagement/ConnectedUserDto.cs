namespace Social.Application.FollowManagement;

// One row of the followers or the following list.
public sealed record ConnectedUserDto(Guid UserId, string DisplayName);
