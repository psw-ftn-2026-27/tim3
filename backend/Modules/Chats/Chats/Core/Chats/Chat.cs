using Chats.Core.Messages;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Chats.Core.Chats;

public sealed class Chat : AggregateRoot
{
    public Subject Subject { get; private set; }
    // P4: every chat keeps its own muted users, until a privileged user unmutes them.
    public List<Guid> MutedUserIds { get; private set; }

    private Chat()
    {
        Subject = null!;
        MutedUserIds = null!;
    }

    public Chat(Subject subject) : base(Guid.NewGuid())
    {
        Subject = subject;
        MutedUserIds = [];
    }

    public Message Write(Guid senderId, string text)
    {
        if (!Subject.Admits(senderId))
        {
            throw new DomainException("Only the two users of a conversation can write in it.");
        }
        if (MutedUserIds.Contains(senderId))
        {
            throw new DomainException("You are muted in this chat.");
        }

        return new Message(Id, senderId, text);
    }

    public void Mute(Guid moderatorId, Guid userId, Privileges privileges)
    {
        privileges.EnsureCanModerate(moderatorId, userId);

        if (!MutedUserIds.Contains(userId))
        {
            MutedUserIds.Add(userId);
        }
    }

    public void Unmute(Guid moderatorId, Guid userId, Privileges privileges)
    {
        if (!privileges.Includes(moderatorId))
        {
            throw new DomainException("Only a privileged user can moderate this chat.");
        }

        MutedUserIds.Remove(userId);
    }
}
