using Exploration.Contracts;
using Payment.Application.ShoppingCart;

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
}
