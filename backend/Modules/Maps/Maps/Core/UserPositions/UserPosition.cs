using Shared.Domain;

namespace Maps.Core.UserPositions;

public sealed class UserPosition : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Coordinates Coordinates { get; private set; }
    public DateTime SetAt { get; private set; }

    private UserPosition()
    {
        Coordinates = null!;
    }

    public UserPosition(Guid userId, Coordinates coordinates, DateTime setAt) : base(Guid.NewGuid())
    {
        UserId = userId;
        Coordinates = coordinates;
        SetAt = setAt;
    }

    public void MoveTo(Coordinates coordinates, DateTime setAt)
    {
        Coordinates = coordinates;
        SetAt = setAt;
    }
}
