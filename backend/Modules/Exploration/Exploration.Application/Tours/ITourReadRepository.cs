using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize);
    Task<bool> IsPublishedAsync(Guid tourId);
    Task<Guid?> GetAuthorIdAsync(Guid tourId);
    Task<TourKeyPointsDto?> GetKeyPointsAsync(Guid tourId, Guid authorId);
    Task<TourPreviewDto?> GetPreviewAsync(Guid tourId, Guid userId);
}
