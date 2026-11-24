namespace Payment.Application.Products;

public interface ITourAvailability
{
    Task<bool> IsAvailableAsync(Guid tourId);

    // The subset of the given tours that are on offer.
    Task<HashSet<Guid>> GetAvailableTourIdsAsync(IEnumerable<Guid> tourIds);
}
