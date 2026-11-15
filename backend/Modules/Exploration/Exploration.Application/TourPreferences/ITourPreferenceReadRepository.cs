using Exploration.Application.TourPreferenceManagement;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceReadRepository
{
    Task<TourPreferenceDto?> GetByExplorerIdAsync(Guid explorerId);
}
