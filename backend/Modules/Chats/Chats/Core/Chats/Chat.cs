using Chats.Core.Messages;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Chats.Core.Chats;

public sealed class Chat : AggregateRoot
{
    public Subject Subject { get; private set; }
    // P4: every chat keeps its own muted users, until a privileged user unmutes them.
    // The list changes only through Mute and Unmute, so outside code gets a read-only view.
    private readonly List<Guid> _mutedUserIds = [];
    public IReadOnlyList<Guid> MutedUserIds => _mutedUserIds.AsReadOnly();

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
        if (MutedUserIds.Contains(senderId))
        {
            throw new DomainException("You are muted in this chat.");
        }

        return new Message(Id, senderId, text);
    }

    public void Mute(Guid moderatorId, Guid userId, Privileges privileges)
    {
        privileges.EnsureCanModerate(moderatorId, userId);

        if (!_mutedUserIds.Contains(userId))
        {
            _mutedUserIds.Add(userId);
        }
    }

    public void Unmute(Guid moderatorId, Guid userId, Privileges privileges)
    {
        if (!privileges.Includes(moderatorId))
        {
            throw new DomainException("Only a privileged user can moderate this chat.");
        }

        _mutedUserIds.Remove(userId);
    }
}
