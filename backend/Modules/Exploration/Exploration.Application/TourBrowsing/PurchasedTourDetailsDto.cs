using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record PurchasedTourDetailsDto(TourDto Tour, List<PreviewKeyPointDto> KeyPoints);
