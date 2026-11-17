using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public Task<Guid?> GetAuthorIdAsync(Guid tourId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId)
            .Select(tour => (Guid?)tour.AuthorId)
            .SingleOrDefaultAsync();
    }

    public Task<bool> IsPublishedAsync(Guid tourId)
    {
        return PublishedTours().AnyAsync(tour => tour.Id == tourId);
    }

    public Task<TourDetailsDto?> GetPublishedDetailsAsync(Guid tourId)
    {
        return PublishedTours()
            .Where(tour => tour.Id == tourId)
            .Select(tour => new TourDetailsDto(tour.Name, tour.Description, null))
            .SingleOrDefaultAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
    {
        var published = PublishedTours().OrderByDescending(tour => tour.PublishedAt);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(published)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    private IQueryable<Tour> PublishedTours()
    {
        return _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published);
    }

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.ArchivedAt,
                tour.RecommendedGroupSize == null
                    ? null
                    : new GroupSizeDto(tour.RecommendedGroupSize.From, tour.RecommendedGroupSize.To),
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList()));
    }
}
