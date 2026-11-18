using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourPreviewDto(
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    GroupSizeDto? RecommendedGroupSize,
    List<TransportTimeDto> TransportTimes,
    TourLengthDto Length,
    TourStatus Status,
    bool IsOwnTour,
    List<PreviewKeyPointDto> KeyPoints);
