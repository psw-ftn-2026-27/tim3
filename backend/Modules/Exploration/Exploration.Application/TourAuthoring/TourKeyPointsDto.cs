using Exploration.Domain.Tours;

namespace Exploration.Application.TourAuthoring;

public sealed record TourKeyPointsDto(
    Guid Id,
    string Name,
    TourStatus Status,
    List<KeyPointDto> KeyPoints,
    TourLengthDto Length);
