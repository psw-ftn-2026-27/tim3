namespace Exploration.Contracts;

// The only part of Exploration other modules may see. A tour appears here only by its ID.
public interface IExplorationApi
{
    // The author of the tour, or null when no tour has that ID.
    Task<Guid?> GetTourAuthorIdAsync(Guid tourId);
}
