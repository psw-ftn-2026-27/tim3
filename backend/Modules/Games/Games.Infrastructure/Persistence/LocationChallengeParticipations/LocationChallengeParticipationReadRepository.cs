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
        var explorerParticipations = _dbContext.LocationChallengeParticipations
            .Where(participation => participation.ExplorerId == explorerId);
        var locationChallenges = _dbContext.LocationChallenges
            .OrderBy(locationChallenge => locationChallenge.Name)
            .ThenBy(locationChallenge => locationChallenge.Id);

        var totalCount = await locationChallenges.CountAsync();
        var rows = await locationChallenges
            .Select(locationChallenge => new
            {
                locationChallenge.Id,
                locationChallenge.Name,
                locationChallenge.StartLocation.Latitude,
                locationChallenge.StartLocation.Longitude,
                locationChallenge.ExperienceGained,
                Status = explorerParticipations
                    .Where(participation => participation.LocationChallengeId == locationChallenge.Id)
                    .Select(participation => (LocationChallengeParticipationStatus?)participation.Status)
                    .FirstOrDefault()
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows
            .Select(row => new LocationChallengeToSolveDto(row.Id, row.Name, row.Latitude, row.Longitude,
                row.ExperienceGained, ToState(row.Status)))
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
                SearchCircle = new SearchCircleDto(
                    participation.SearchCircle.Center.Latitude,
                    participation.SearchCircle.Center.Longitude,
                    participation.SearchCircle.Radius),
                participation.DiscoveredPointIds
            })
            .FirstOrDefaultAsync();
        var discoveredPointIds = participation?.DiscoveredPointIds.ToList() ?? [];

        var locationChallenge = await _dbContext.LocationChallenges
            .Where(locationChallenge => locationChallenge.Id == locationChallengeId)
            .Select(locationChallenge => new
            {
                locationChallenge.Id,
                locationChallenge.Name,
                locationChallenge.ExperienceGained,
                HiddenPoints = locationChallenge.HiddenPoints
                    .Select(hiddenPoint => new HiddenPointClueDto(
                        hiddenPoint.Id,
                        hiddenPoint.Photo,
                        discoveredPointIds.Contains(hiddenPoint.Id),
                        discoveredPointIds.Contains(hiddenPoint.Id) ? hiddenPoint.Location.Latitude : null,
                        discoveredPointIds.Contains(hiddenPoint.Id) ? hiddenPoint.Location.Longitude : null))
                    .ToList()
            })
            .FirstOrDefaultAsync();
        if (locationChallenge is null)
        {
            return null;
        }

        return new LocationChallengeHuntDto(locationChallenge.Id, locationChallenge.Name,
            locationChallenge.ExperienceGained, ToState(participation?.Status),
            participation?.SearchCircle, locationChallenge.HiddenPoints);
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
