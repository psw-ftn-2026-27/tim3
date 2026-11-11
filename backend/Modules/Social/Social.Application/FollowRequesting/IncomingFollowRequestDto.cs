namespace Social.Application.FollowRequesting;

public sealed record IncomingFollowRequestDto(Guid Id, Guid SenderId, string SenderDisplayName, DateTime SentAt);
