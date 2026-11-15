using Exploration.Application.TourPreferences;

namespace Exploration.Application.TourPreferenceManagement;

public sealed class TourPreferenceManagementQueries
{
    private readonly ITourPreferenceReadRepository _preferenceReadRepository;

    public TourPreferenceManagementQueries(ITourPreferenceReadRepository preferenceReadRepository)
    {
        _preferenceReadRepository = preferenceReadRepository;
    }

    public Task<TourPreferenceDto?> GetByExplorerAsync(Guid explorerId)
    {
        return _preferenceReadRepository.GetByExplorerIdAsync(explorerId);
    }
}
