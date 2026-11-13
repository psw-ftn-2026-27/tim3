using Exploration.Domain.Tours;

namespace Exploration.Application.TourPreferenceManagement;

public sealed record TourPreferenceDto(
    Guid Id,
    Guid ExplorerId,
    List<TourDifficulty> Difficulties,
    TransportMode? Transport,
    List<string> Tags,
    bool AppliesToSearch);
