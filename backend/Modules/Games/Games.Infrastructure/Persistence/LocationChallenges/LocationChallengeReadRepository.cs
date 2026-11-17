using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeReadRepository : ILocationChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<LocationChallengeSummaryDto>> GetPageAsync(int page, int pageSize)
    {
        var locationChallenges = _dbContext.LocationChallenges
            .OrderBy(locationChallenge => locationChallenge.Name)
            .ThenBy(locationChallenge => locationChallenge.Id);

        var totalCount = await locationChallenges.CountAsync();
        var items = await locationChallenges
            .Select(locationChallenge => new LocationChallengeSummaryDto(
                locationChallenge.Id,
                locationChallenge.Name,
                locationChallenge.ExperienceGained,
                locationChallenge.HiddenPoints.Count))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<LocationChallengeSummaryDto>(items, totalCount);
    }

    public Task<LocationChallengeDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.LocationChallenges
            .Where(locationChallenge => locationChallenge.Id == id)
            .Select(locationChallenge => new LocationChallengeDto(
                locationChallenge.Id,
                locationChallenge.Name,
                locationChallenge.StartLocation.Latitude,
                locationChallenge.StartLocation.Longitude,
                locationChallenge.ExperienceGained,
                locationChallenge.HiddenPoints
                    .Select(hiddenPoint => new HiddenPointDto(
                        hiddenPoint.Id,
                        hiddenPoint.Photo,
                        hiddenPoint.Location.Latitude,
                        hiddenPoint.Location.Longitude))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
