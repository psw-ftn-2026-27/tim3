namespace Chats.Core.ChatMessaging;

// SenderIsPrivileged tells the client not to offer moderation next to this message (P5).
public sealed record MessageDto(
    Guid Id, Guid SenderId, string SenderName, string Text, DateTime SentAt, bool SenderIsPrivileged);
