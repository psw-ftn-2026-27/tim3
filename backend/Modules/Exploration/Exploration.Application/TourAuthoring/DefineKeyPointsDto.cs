namespace Exploration.Application.TourAuthoring;

public sealed record DefineKeyPointsDto(List<KeyPointInputDto> KeyPoints, TourLengthDto Length);
