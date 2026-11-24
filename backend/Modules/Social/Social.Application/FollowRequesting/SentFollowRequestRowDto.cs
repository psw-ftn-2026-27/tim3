using Social.Domain.Profiles;

namespace Social.Application.FollowRequesting;

// What the read repository knows about a sent request: user IDs only. The display name
// comes from Identity, so FollowRequestingQueries turns this into a SentFollowRequestDto.
public sealed record SentFollowRequestRowDto(Guid Id, Guid RecipientId, DateTime SentAt, FollowRequestStatus Status);
