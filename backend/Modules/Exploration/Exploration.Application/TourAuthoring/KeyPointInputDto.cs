using Exploration.Application.Tours;

namespace Exploration.Application.TourAuthoring;

public sealed record KeyPointInputDto(
    string Name,
    string Description,
    string ImageAddress,
    GeoLocationDto? Location,
    string? Secret,
    bool VisibleWithoutPurchase);
