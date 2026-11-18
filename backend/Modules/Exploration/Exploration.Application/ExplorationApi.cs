using Exploration.Application.Tours;
using Exploration.Contracts;

namespace Exploration.Application;

public sealed class ExplorationApi : IExplorationApi
{
    private readonly ITourReadRepository _tourReadRepository;

    public ExplorationApi(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<Guid?> GetTourAuthorIdAsync(Guid tourId)
    {
        return _tourReadRepository.GetAuthorIdAsync(tourId);
    }

    public Task<bool> GetTourAvailabilityAsync(Guid tourId)
    {
        return _tourReadRepository.IsPublishedAsync(tourId);
    }
}
