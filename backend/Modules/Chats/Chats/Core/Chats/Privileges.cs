using Shared.Domain.Exceptions;

namespace Chats.Core.Chats;

// The users who may moderate one chat (P1). The chat never decides who they are: the application
// layer assembles them and hands them in. The set may be empty, hold one user or many (P6).
public sealed class Privileges
{
    private readonly HashSet<Guid> _userIds;

    public Privileges(IEnumerable<Guid> userIds)
    {
        _userIds = userIds.ToHashSet();
    }

    public bool Includes(Guid userId)
    {
        return _userIds.Contains(userId);
    }

    // P2: only a privileged user moderates. P5: a privileged user is never the target.
    public void EnsureCanModerate(Guid moderatorId, Guid targetUserId)
    {
        if (!Includes(moderatorId))
        {
            throw new DomainException("Only a privileged user can moderate this chat.");
        }
        if (Includes(targetUserId))
        {
            throw new DomainException("A privileged user cannot be moderated in this chat.");
        }
    }
}
