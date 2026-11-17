namespace Chats.Core.ChatMessaging;

// A stored message before the sender's display name is added from Identity.
public sealed record MessageRowDto(Guid Id, Guid SenderId, string Text, DateTime SentAt);
