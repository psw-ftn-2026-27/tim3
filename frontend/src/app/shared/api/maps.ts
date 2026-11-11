export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface WalkingRouteDto {
  points: LocationDto[];
  lengthInKilometres: number;
}
