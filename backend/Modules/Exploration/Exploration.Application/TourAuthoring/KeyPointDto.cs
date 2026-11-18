namespace Exploration.Application.TourAuthoring;

public sealed record KeyPointDto(
    Guid Id,
    int Order,
    string Name,
    string Description,
    string ImageAddress,
    GeoLocationDto Location,
    string? Secret,
    bool VisibleWithoutPurchase);
