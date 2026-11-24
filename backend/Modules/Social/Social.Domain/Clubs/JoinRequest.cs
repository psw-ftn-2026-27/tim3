namespace Social.Domain.Clubs;

public sealed record JoinRequest
{
    public Guid UserId { get; private init; }

    private JoinRequest() { }

    public JoinRequest(Guid userId)
    {
        UserId = userId;
    }
}
