using Shared.Domain.Exceptions;

namespace Chats.Core.Chats;

// What a chat belongs to: one tour, one challenge, or one pair of users.
// Each kind has its own named method, so a tour with two IDs or a pair with one cannot be written.
public sealed record Subject
{
    public SubjectKind Kind { get; private init; }
    public Guid FirstId { get; private init; }
    public Guid? SecondId { get; private init; }

    private Subject() { }

    private Subject(SubjectKind kind, Guid firstId, Guid? secondId)
    {
        Kind = kind;
        FirstId = firstId;
        SecondId = secondId;
    }

    public static Subject ForTour(Guid tourId) => new(SubjectKind.Tour, tourId, null);

    public static Subject ForChallenge(Guid challengeId) => new(SubjectKind.Challenge, challengeId, null);

    public static Subject ForPair(Guid oneUserId, Guid otherUserId)
    {
        if (oneUserId == otherUserId)
        {
            throw new DomainException("A conversation needs two different users.");
        }

        // The smaller ID always comes first, so (A, B) and (B, A) are the same subject.
        return oneUserId.CompareTo(otherUserId) < 0
            ? new Subject(SubjectKind.Pair, oneUserId, otherUserId)
            : new Subject(SubjectKind.Pair, otherUserId, oneUserId);
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
