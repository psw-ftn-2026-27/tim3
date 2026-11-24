export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface GroupSizeDto {
  from: number;
  to: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  archivedAt: string | null;
  recommendedGroupSize: GroupSizeDto | null;
  transportTimes: TransportTimeDto[];
}

export interface PublishedTourDto extends TourDto {
  priceInEc: number | null;
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: GroupSizeDto | null;
}

export interface PublishTourDto {
  isFree: boolean | null;
  amount: number | null;
}

export interface GeoLocationDto {
  latitude: number;
  longitude: number;
}

export interface TourLengthDto {
  kilometres: number;
  isStraightLine: boolean;
}

export interface KeyPointDto {
  id: string;
  order: number;
  name: string;
  description: string;
  imageAddress: string;
  location: GeoLocationDto;
  secret: string | null;
  visibleWithoutPurchase: boolean;
}

export interface TourKeyPointsDto {
  id: string;
  name: string;
  status: TourStatus;
  keyPoints: KeyPointDto[];
  length: TourLengthDto;
}

export interface KeyPointInputDto {
  name: string;
  description: string;
  imageAddress: string;
  location: GeoLocationDto | null;
  secret: string | null;
  visibleWithoutPurchase: boolean;
}

export interface DefineKeyPointsDto {
  keyPoints: KeyPointInputDto[];
  length: TourLengthDto;
}

export interface PreviewKeyPointDto {
  name: string;
  description: string;
  imageAddress: string;
  location: GeoLocationDto;
}

export interface TourPreviewDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: GroupSizeDto | null;
  transportTimes: TransportTimeDto[];
  length: TourLengthDto;
  status: TourStatus;
  isOwnTour: boolean;
  keyPoints: PreviewKeyPointDto[];
  priceInEc: number | null;
}

export interface PurchasedTourDetailsDto {
  tour: TourDto;
  keyPoints: PreviewKeyPointDto[];
}
