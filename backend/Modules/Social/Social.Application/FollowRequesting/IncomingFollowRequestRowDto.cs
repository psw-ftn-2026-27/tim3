namespace Social.Application.FollowRequesting;

// What the read repository knows about an incoming request: user IDs only. The display name
// comes from Identity, so FollowRequestingQueries turns this into an IncomingFollowRequestDto.
public sealed record IncomingFollowRequestRowDto(Guid Id, Guid SenderId, DateTime SentAt);
