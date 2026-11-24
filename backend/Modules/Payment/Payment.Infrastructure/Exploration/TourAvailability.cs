using Exploration.Contracts;
using Payment.Application.Products;

namespace Payment.Infrastructure.Exploration;

internal sealed class TourAvailability : ITourAvailability
{
    private readonly IExplorationApi _exploration;

    public TourAvailability(IExplorationApi exploration)
    {
        _exploration = exploration;
    }

    public Task<bool> IsAvailableAsync(Guid tourId)
    {
        return _exploration.GetTourAvailabilityAsync(tourId);
    }

    // Exploration's contract answers one tour at a time, so the loop lives here, once.
    public async Task<HashSet<Guid>> GetAvailableTourIdsAsync(IEnumerable<Guid> tourIds)
    {
        var available = new HashSet<Guid>();
        foreach (var tourId in tourIds)
        {
            if (await _exploration.GetTourAvailabilityAsync(tourId))
            {
                available.Add(tourId);
            }
        }
        return available;
    }
}
