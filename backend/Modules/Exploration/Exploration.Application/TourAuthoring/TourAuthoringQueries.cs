using Exploration.Application.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public TourAuthoringQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return _tourReadRepository.GetByAuthorAsync(authorId);
    }

    public async Task<TourKeyPointsDto> GetKeyPointsAsync(Guid tourId, Guid authorId)
    {
        var keyPoints = await _tourReadRepository.GetKeyPointsAsync(tourId, authorId);
        if (keyPoints is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        return keyPoints;
    }
}
