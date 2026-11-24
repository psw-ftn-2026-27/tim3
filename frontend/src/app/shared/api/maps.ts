export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface WalkingRouteDto {
  points: LocationDto[];
  lengthInKilometres: number;
}

export interface SetUserPositionDto {
  latitude: number;
  longitude: number;
}

export interface UserPositionDto {
  latitude: number;
  longitude: number;
  setAt: string;
}
