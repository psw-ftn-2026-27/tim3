using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record PublishedTourDto(
    Guid Id,
    Guid AuthorId,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    TourStatus Status,
    DateTime? PublishedAt,
    DateTime? ArchivedAt,
    GroupSizeDto? RecommendedGroupSize,
    List<TransportTimeDto> TransportTimes,
    int? PriceInEc);
