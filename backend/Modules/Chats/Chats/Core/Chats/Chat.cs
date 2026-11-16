using Chats.Core.Messages;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Chats.Core.Chats;

public sealed class Chat : AggregateRoot
{
    public Subject Subject { get; private set; }

    private Chat()
    {
        Subject = null!;
    }

    public Chat(Subject subject) : base(Guid.NewGuid())
    {
        Subject = subject;
    }

    public Message Write(Guid senderId, string text)
    {
        if (!Subject.Admits(senderId))
        {
            throw new DomainException("Only the two users of a conversation can write in it.");
        }

        return new Message(Id, senderId, text);
    }
}
