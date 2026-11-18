using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record PreviewKeyPointDto(
    string Name,
    string Description,
    string ImageAddress,
    GeoLocationDto Location);
