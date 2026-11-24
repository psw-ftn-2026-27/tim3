namespace Games.Application.LocationChallengeSolving;

public sealed record HiddenPointClueDto(Guid Id, string Photo, bool Discovered, double? Latitude, double? Longitude);
