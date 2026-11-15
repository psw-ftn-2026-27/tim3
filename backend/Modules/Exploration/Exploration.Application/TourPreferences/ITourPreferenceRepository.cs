using Exploration.Domain.TourPreferences;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceRepository
{
    Task<TourPreference?> GetByExplorerIdAsync(Guid explorerId);
    void Add(TourPreference preference);
    void Remove(TourPreference preference);
}
