namespace Maps.Core.UserPositions;

public interface IUserPositionRepository
{
    Task<UserPosition?> GetByUserIdAsync(Guid userId);
    void Add(UserPosition userPosition);
}
