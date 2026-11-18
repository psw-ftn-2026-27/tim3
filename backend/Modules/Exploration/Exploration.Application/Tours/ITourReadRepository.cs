using Exploration.Application.TourAuthoring;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize);
    Task<Guid?> GetAuthorIdAsync(Guid tourId);
    Task<TourKeyPointsDto?> GetKeyPointsAsync(Guid tourId, Guid authorId);
}
