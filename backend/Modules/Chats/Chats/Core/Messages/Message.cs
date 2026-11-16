using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Chats.Core.Messages;

public sealed class Message : AggregateRoot
{
    public const int MaxTextLength = 1000;

    public Guid ChatId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Text { get; private set; }
    public DateTime SentAt { get; private set; }

    private Message()
    {
        Text = null!;
    }

    public Message(Guid chatId, Guid senderId, string text) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A message cannot be empty.");
        }
        if (text.Length > MaxTextLength)
        {
            throw new DomainException($"A message cannot be longer than {MaxTextLength} characters.");
        }

        ChatId = chatId;
        SenderId = senderId;
        Text = text;
        SentAt = DateTime.UtcNow;
    }
}
