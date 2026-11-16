using Shared.Domain.Exceptions;

namespace Chats.Core.Chats;

// What a chat belongs to: one tour, one challenge, or one pair of users.
public sealed record Subject
{
    public SubjectKind Kind { get; private init; }
    public Guid FirstId { get; private init; }
    public Guid? SecondId { get; private init; }

    private Subject() { }

    public Subject(SubjectKind kind, Guid id, Guid? secondId = null)
    {
        if (kind != SubjectKind.Pair)
        {
            if (secondId is not null)
            {
                throw new DomainException("A tour or challenge chat belongs to exactly one tour or challenge.");
            }

            Kind = kind;
            FirstId = id;
            return;
        }

        if (secondId is null || secondId.Value == id)
        {
            throw new DomainException("A conversation needs two different users.");
        }

        // The smaller ID always comes first, so (A, B) and (B, A) are the same subject.
        var idComesFirst = id.CompareTo(secondId.Value) < 0;
        Kind = kind;
        FirstId = idComesFirst ? id : secondId.Value;
        SecondId = idComesFirst ? secondId.Value : id;
    }

    public bool Admits(Guid userId)
    {
        if (Kind == SubjectKind.Pair)
        {
            return userId == FirstId || userId == SecondId;
        }

        return true;
    }
}
