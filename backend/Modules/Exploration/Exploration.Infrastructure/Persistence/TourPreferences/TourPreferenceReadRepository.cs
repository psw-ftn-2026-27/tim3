using Exploration.Application.TourPreferenceManagement;
using Exploration.Application.TourPreferences;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal sealed class TourPreferenceReadRepository : ITourPreferenceReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourPreferenceReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourPreferenceDto?> GetByExplorerIdAsync(Guid explorerId)
    {
        return _dbContext.TourPreferences
            .Where(preference => preference.ExplorerId == explorerId)
            .Select(preference => new TourPreferenceDto(
                preference.Id,
                preference.ExplorerId,
                preference.Difficulties,
                preference.Transport,
                preference.Tags,
                preference.AppliesToSearch))
            .FirstOrDefaultAsync();
    }
}
