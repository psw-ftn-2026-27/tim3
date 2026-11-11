using Social.Domain.Profiles;

namespace Social.Application.FollowRequesting;

public sealed record SentFollowRequestDto(
    Guid Id,
    Guid RecipientId,
    string RecipientDisplayName,
    DateTime SentAt,
    FollowRequestStatus Status);
