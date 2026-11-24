namespace Maps.Contracts;

// The only part of Maps other modules may see. A position appears here only as coordinates
// and the time it was set, never as the way it was set, so its source can change freely.
public interface IMapsApi
{
    Task<CurrentPosition?> GetCurrentPositionAsync(Guid userId);
}
