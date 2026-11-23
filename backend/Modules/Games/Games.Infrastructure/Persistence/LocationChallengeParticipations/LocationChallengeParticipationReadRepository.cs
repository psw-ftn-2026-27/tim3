using Games.Application.LocationChallengeParticipations;
using Games.Application.LocationChallengeSolving;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.LocationChallengeParticipations;

internal sealed class LocationChallengeParticipationReadRepository : ILocationChallengeParticipationReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeParticipationReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<LocationChallengeToSolveDto>> GetPageAsync(Guid explorerId, int page, int pageSize)
    {
        var totalCount = await _dbContext.LocationChallenges.CountAsync();
        var locationChallenges = await _dbContext.LocationChallenges
            .OrderBy(locationChallenge => locationChallenge.Name)
            .ThenBy(locationChallenge => locationChallenge.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(locationChallenge => new
            {
                locationChallenge.Id,
                locationChallenge.Name,
                locationChallenge.StartLocation.Latitude,
                locationChallenge.StartLocation.Longitude,
                locationChallenge.ExperienceGained
            })
            .ToListAsync();
        var statuses = await GetStatusesAsync(explorerId, locationChallenges.Select(locationChallenge => locationChallenge.Id).ToList());

        var items = locationChallenges
            .Select(locationChallenge => new LocationChallengeToSolveDto(locationChallenge.Id, locationChallenge.Name,
                locationChallenge.Latitude, locationChallenge.Longitude, locationChallenge.ExperienceGained,
                statuses.TryGetValue(locationChallenge.Id, out var status) ? ToState(status) : LocationChallengeState.NotStarted))
            .ToList();

        return new PageResult<LocationChallengeToSolveDto>(items, totalCount);
    }

    public async Task<LocationChallengeHuntDto?> GetByIdAsync(Guid explorerId, Guid locationChallengeId)
    {
        var participation = await _dbContext.LocationChallengeParticipations
            .Where(participation => participation.ExplorerId == explorerId
                && participation.LocationChallengeId == locationChallengeId)
            .Select(participation => new
            {
                participation.Status,
                SearchCircle = new SearchCircleDto(participation.SearchCircle.Center.Latitude,
                    participation.SearchCircle.Center.Longitude, participation.SearchCircle.Radius),
                participation.DiscoveredPointIds
            })
            .FirstOrDefaultAsync();
        var discoveredPointIds = participation?.DiscoveredPointIds.ToList() ?? [];

        var locationChallenge = await _dbContext.LocationChallenges
            .Where(locationChallenge => locationChallenge.Id == locationChallengeId)
            .Select(locationChallenge => new { locationChallenge.Id, locationChallenge.Name, locationChallenge.ExperienceGained })
            .FirstOrDefaultAsync();
        if (locationChallenge is null)
        {
            return null;
        }
        var hiddenPoints = await GetHiddenPointCluesAsync(locationChallengeId, discoveredPointIds);

        return new LocationChallengeHuntDto(locationChallenge.Id, locationChallenge.Name,
            locationChallenge.ExperienceGained, ToState(participation?.Status),
            participation?.SearchCircle, hiddenPoints);
    }

    // The status of each listed challenge for this explorer; a challenge without a participation is missing.
    private async Task<Dictionary<Guid, LocationChallengeParticipationStatus>> GetStatusesAsync(Guid explorerId,
        List<Guid> locationChallengeIds)
    {
        return await _dbContext.LocationChallengeParticipations
            .Where(participation => participation.ExplorerId == explorerId
                && locationChallengeIds.Contains(participation.LocationChallengeId))
            .ToDictionaryAsync(participation => participation.LocationChallengeId, participation => participation.Status);
    }

    // P6: the query selects coordinates only for discovered points, so undiscovered locations never leave the database.
    private async Task<List<HiddenPointClueDto>> GetHiddenPointCluesAsync(Guid locationChallengeId, List<Guid> discoveredPointIds)
    {
        return await _dbContext.LocationChallenges
            .Where(locationChallenge => locationChallenge.Id == locationChallengeId)
            .SelectMany(locationChallenge => locationChallenge.HiddenPoints)
            .Select(hiddenPoint => new HiddenPointClueDto(
                hiddenPoint.Id,
                hiddenPoint.Photo,
                discoveredPointIds.Contains(hiddenPoint.Id),
                discoveredPointIds.Contains(hiddenPoint.Id) ? hiddenPoint.Location.Latitude : null,
                discoveredPointIds.Contains(hiddenPoint.Id) ? hiddenPoint.Location.Longitude : null))
            .ToListAsync();
    }

    private static LocationChallengeState ToState(LocationChallengeParticipationStatus? status)
    {
        return status switch
        {
            null => LocationChallengeState.NotStarted,
            LocationChallengeParticipationStatus.Active => LocationChallengeState.Active,
            _ => LocationChallengeState.Solved
        };
    }
}
