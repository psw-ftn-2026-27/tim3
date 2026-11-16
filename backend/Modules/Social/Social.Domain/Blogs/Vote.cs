namespace Social.Domain.Blogs;

public sealed record Vote
{
    public Guid UserId { get; private init; }
    public VoteType VoteType { get; private init; }
    public DateTime VotedAt { get; private init; }

    private Vote() { }

    public Vote(Guid userId, VoteType voteType)
    {
        UserId = userId;
        VoteType = voteType;
        VotedAt = DateTime.UtcNow;
    }
}
