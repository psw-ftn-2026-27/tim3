namespace Maps.Core.WalkingRoutes;

public sealed record WalkingRouteDto(IReadOnlyList<LocationDto> Points, double LengthInKilometres);
