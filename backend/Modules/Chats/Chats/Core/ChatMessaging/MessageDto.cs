namespace Chats.Core.ChatMessaging;

public sealed record MessageDto(Guid Id, Guid SenderId, string SenderName, string Text, DateTime SentAt);
