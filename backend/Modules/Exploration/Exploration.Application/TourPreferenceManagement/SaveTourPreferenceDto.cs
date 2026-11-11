using Exploration.Domain.Tours;

namespace Exploration.Application.TourPreferenceManagement;

public sealed record SaveTourPreferenceDto(
    List<TourDifficulty> Difficulties,
    TransportMode? Transport,
    List<string> Tags,
    bool AppliesToSearch);
