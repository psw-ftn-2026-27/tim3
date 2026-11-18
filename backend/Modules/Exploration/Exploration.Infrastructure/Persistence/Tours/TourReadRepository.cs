using Exploration.Application.TourAuthoring;
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

    public Task<TourDto?> GetByIdAsync(Guid tourId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.Id == tourId)).SingleOrDefaultAsync();
    }

    public Task<List<PreviewKeyPointDto>> GetPurchasedKeyPointsAsync(Guid tourId)
    {
        return _dbContext.Tours.Where(tour => tour.Id == tourId)
            .SelectMany(tour => tour.KeyPoints)
            .OrderBy(point => point.Order)
            .Select(point => new PreviewKeyPointDto(
                point.Name, point.Description, point.ImageAddress,
                new GeoLocationDto(point.Location.Latitude, point.Location.Longitude)))
            .ToListAsync();
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

    public Task<TourKeyPointsDto?> GetKeyPointsAsync(Guid tourId, Guid authorId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.AuthorId == authorId)
            .Select(tour => new TourKeyPointsDto(
                tour.Id,
                tour.Name,
                tour.Status,
                tour.KeyPoints
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new KeyPointDto(
                        keyPoint.Id,
                        keyPoint.Order,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.ImageAddress,
                        new GeoLocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude),
                        keyPoint.Secret,
                        keyPoint.VisibleWithoutPurchase))
                    .ToList(),
                new TourLengthDto(tour.Length.Kilometres, tour.Length.IsStraightLine)))
            .SingleOrDefaultAsync();
    }

    public Task<TourPreviewDto?> GetPreviewAsync(Guid tourId, Guid userId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && (tour.Status == TourStatus.Published || tour.AuthorId == userId))
            .Select(tour => new TourPreviewDto(
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.RecommendedGroupSize == null
                    ? null
                    : new GroupSizeDto(tour.RecommendedGroupSize.From, tour.RecommendedGroupSize.To),
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                new TourLengthDto(tour.Length.Kilometres, tour.Length.IsStraightLine),
                tour.Status,
                tour.AuthorId == userId,
                tour.KeyPoints
                    .Where(keyPoint => keyPoint.VisibleWithoutPurchase)
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new PreviewKeyPointDto(
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.ImageAddress,
                        new GeoLocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                    .ToList()))
            .SingleOrDefaultAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
    {
        var published = PublishedTours().OrderByDescending(tour => tour.PublishedAt);
        return await GetPageAsync(published, page, pageSize);
    }

    public Task<PageResult<TourDto>> GetByIdsAsync(List<Guid> tourIds, int page, int pageSize)
    {
        var purchased = _dbContext.Tours.Where(tour => tourIds.Contains(tour.Id))
            .OrderBy(tour => tour.Name)
            .ThenBy(tour => tour.Id);
        return GetPageAsync(purchased, page, pageSize);
    }

    private static async Task<PageResult<TourDto>> GetPageAsync(IQueryable<Tour> tours, int page, int pageSize)
    {
        var totalCount = await tours.CountAsync();
        if ((long)(page - 1) * pageSize >= totalCount)
        {
            return new PageResult<TourDto>([], totalCount);
        }

        var items = await ProjectToDtos(tours)
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
