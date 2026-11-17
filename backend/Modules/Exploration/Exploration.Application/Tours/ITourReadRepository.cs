using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize);
    Task<TourDetailsDto?> GetPublishedDetailsAsync(Guid tourId);
    Task<bool> IsPublishedAsync(Guid tourId);
    Task<Guid?> GetAuthorIdAsync(Guid tourId);
}
